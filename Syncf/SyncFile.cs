using Fred68.CfgReader;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Syncf
{
	
	public class SyncFile
	{
		public delegate void FuncMsg(string msg);

		string cfgName = "Syncf.cfg";
		dynamic cfg;
		string userName = string.Empty;
		bool enabled;
		FuncMsg fmsg;
		string busyFile = string.Empty;


		#region Proprieta'
		
		public bool isEnabled
		{
			get { return enabled; }
		}

		public FuncMsg Fmsg
		{
			get {return fmsg;}
		}
		
		public FuncBkgnd? StartFunc()
		{
			FuncBkgnd? f = null;
			switch(cfg.command)
			{
				case 0:
					break;
				case 1:
					fmsg("\r\nAVVIO AUTOMATICO: lettura file\r\n");
					f = ReadFile;
					break;
				case 2:
					fmsg("\r\nAVVIO AUTOMATICO: scrittura file\r\n");
					f = WriteFile;
					break;
				case 3:
					fmsg("\r\nAVVIO AUTOMATICO: lettura e scrittura file\r\n");
					f = ReadWriteFile;
					break;
				default:
					break;

			}
			return f;
		}
		#endregion

		/// <summary>
		/// Ctor
		/// </summary>
		public SyncFile(FuncMsg f)
		{
			cfg = new CfgReader();
			fmsg = f;
			//MessageBox.Show(cfg.CHR_Ammessi);
			cfg.ReadConfiguration(cfgName);
			userName = Environment.UserName;			// System.Security.Principal.WindowsIdentity.GetCurrent().Name
			enabled = CheckCfg();
		}
		
		public string CfgMsgs()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(cfg.ToString());
			sb.AppendLine(cfg.DumpEntries());
			sb.AppendLine($"User: {userName}");
			return sb.ToString();
		}

		public bool CheckCfg()
		{
			bool ok = true;
			string sTmp;
			List<string> lsTmp;
			int iTmp;

			// Controlla esistenza delle opzioni necessarie
			try
			{	
				sTmp = cfg.extBusy;
				sTmp = cfg.logF;
				sTmp = cfg.todoF;
				sTmp = cfg.doneF;
				sTmp = cfg.indxF;
				iTmp = cfg.command;
				sTmp = cfg.origRoot;
				sTmp = cfg.destRoot;
				lsTmp = cfg.extYes;
				lsTmp = cfg.extNo;
				sTmp = cfg.logPath;
				if(userName.Length < 1)	throw new Exception("Utente non definito");
					
			}
			catch(Exception ex)
			{
				ok = false;
				MessageBox.Show($"Errore nella configurazione\r\n{ex.Message.ToString()}");
			}

			#if DEBUG
			Process.Start("explorer.exe" , cfg.logPath);
			#endif

			// Controlla esistenza della cartella di log
			if(ok)
			{
				if(!CheckLogDir())
				{
					ok = false;
					MessageBox.Show($"La directory di log {cfg.logPath} non esiste.\r\n");
				}
			}

			// Controlla che non ci siano file 'busy' nella cartella di log
			if(ok)
			{
				string pattern = "";
				try
				{
					pattern = $"*.{cfg.extBusy}";
					string[] fs = Directory.GetFiles(cfg.logPath,pattern);
					if(fs.Length > 0)
					{
						ok = false;
						MessageBox.Show($"Sono presenti dei file '{pattern}' nella cartella '{cfg.logPath}'\r\nIl programma è probabilmente utilizzato da altri utenti\r\n");
					}
				}
				catch(Exception ex)
				{
					ok = false;
					MessageBox.Show($"Errore nella ricerca dei file '{pattern}' nella cartella '{cfg.logPath}'\r\n{ex.Message.ToString()}");
				}

			}

			// Crea il file busy
			if(ok)
			{
				ok = LockBusy();
			}

			return ok;
		}

		bool LockBusy()
		{
			bool ok = false;
			busyFile = cfg.logPath + userName + '.' + cfg.extBusy;
			try
			{
				StreamWriter sw = File.CreateText(busyFile);
				sw.Close();
				ok = true;
			}
			catch (Exception ex)
			{
				ok = false;
				MessageBox.Show($"Errore nella creazione del file '{busyFile}' nella cartella '{cfg.logPath}'\r\n{ex.Message.ToString()}");	
			}
			return ok;
		}

		public void ReleaseBusy()
		{
			if(busyFile != string.Empty)
			{
				try
				{
					File.Delete(busyFile);
				}
				catch(Exception e)
				{
					MessageBox.Show($"Errore nella cancellazione del file '{busyFile}'\r\n{e.Message.ToString()}");
				}
			}
		}

		bool CheckLogDir()
		{
			bool ok = false;
			ok = Directory.Exists(cfg.logPath);
			return ok;
		}

		public bool ReadFile(CancellationToken token)
		{
			bool ok = false;
			for(int i = 0;i < 10;i++)       // Esegue le operazioni
				{
					if(token.IsCancellationRequested)
					{
						ok = false;
						break;          // Esce dal ciclo for
					}
					else
					{
						fmsg('R'+i.ToString());
						Thread.Sleep(500);
						if(i == 9)
							{
							fmsg("\r\n");
							ok = true;
							}
					}
				}
			return ok;
		}

		public bool WriteFile(CancellationToken token)
		{
			bool ok = false;
			for(int i = 0;i < 10;i++)       // Esegue le operazioni
			{
				if(token.IsCancellationRequested)
				{
					ok = false;
					break;          // Esce dal ciclo for
				}
				else
				{
					fmsg('W'+i.ToString());
					Thread.Sleep(500);
					if(i == 9)
					{
						fmsg("\r\n");
						ok = true;
					}
				}
			}
			return ok;
		}

		public bool ReadWriteFile(CancellationToken token)
		{
			bool ok, rok, wok;
			ok = rok = wok = false;
			rok = ReadFile(token);

			if(rok)
			{
				wok = WriteFile(token);
			}
			ok = rok && wok;

			return ok;
		}
	}
}
