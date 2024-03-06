
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.IO;

using Fred68.CfgReader;

namespace Syncf
{

	public class SyncFile
	{
		public delegate void FuncMsg(string msg);
		public static string STD_CFG = "Syncf.cfg";

		SyncfParams par;
		dynamic cfg;
		string stdUserName;
		bool enabled = false;				// true se non ci sono errori di configurazione
		string busyFile = string.Empty;		// File busy
		string logFile = string.Empty;		// File di log
		StreamWriter? swLogFile = null;		// Streamwriter (oppure null)

		#region PROPRIETA'
		
		/// <summary>
		/// Comandi abilitati, nessun errore d configurazione
		/// </summary>
		public bool isEnabled
		{
			get { return enabled; }
		}

		/// <summary>
		/// Delegate per visualizzare i messaggi del task principale
		/// </summary>
		public FuncMsg? funcMessage
		{
			get {return par.fmsg;}
		}

		/// <summary>
		/// Messaggio con la configurazione
		/// </summary>
		public string msgConfiguration
		{
			get
			{
				StringBuilder sb = new StringBuilder();
				sb.AppendLine($"Cfg filename = {par.cfgFile}");
				sb.AppendLine($"User: {par.usrName} [{stdUserName}]");
				switch(par.fls)
				{
					case FLS.ALL:
					{
						sb.AppendLine($"Cerca tutti i file in '{cfg.origRoot}'");
					}
					break;
					case FLS.ALL_LST:
					{
						sb.AppendLine($"Legge tutti i file indice '{cfg.indxF}' in '{cfg.logPath}'");
					}
					break;
					case FLS.LST:
					{
						sb.AppendLine($"Legge il file indice '{par.lstFile}{cfg.indxF}' in '{cfg.logPath}'");
					}
					break;
				}
				if(cfg.extYes.Count == 0)
				{
					sb.AppendLine("Nessuna estensione da includere");
				}
				if(cfg.extNo.Count == 0)
				{
					sb.AppendLine("Nessuna estensione da escludere");
				}

				sb.AppendLine(cfg.ToString());
				sb.AppendLine(cfg.DumpEntries());
				
				return sb.ToString();
			}
		}

		#endregion


		/// <summary>
		/// Ctor
		/// </summary>
		
		public SyncFile(SyncfParams p)
		{
			cfg = new CfgReader();
			par = new SyncfParams();

			par.cfgFile = STD_CFG;
			par.fmsg = p.fmsg;
			#if DEBUG
			//MessageBox.Show(cfg.CHR_Ammessi);
			#endif
			par.usrName = stdUserName = par.lstFile = string.Empty;

			par.fls = p.fls;
			if(p.cfgFile.Length > 2)	par.cfgFile = p.cfgFile;
			cfg.ReadConfiguration(par.cfgFile);
			stdUserName = Environment.UserName;			// System.Security.Principal.WindowsIdentity.GetCurrent().Name ?
			if(p.usrName.Length > 1)
			{
				par.usrName = p.usrName;
			}
			else
			{
				par.usrName = stdUserName;
			}
			
			if(par.fls == FLS.LST)
			{
				par.lstFile = p.lstFile;
			}
			else if(par.fls == FLS.None)					// Se non è specificato alcun file di lista...
			{
				par.lstFile = par.usrName;				// ...usa lo username
				par.fls = FLS.LST;
			}
			
			enabled = CheckCfg();

			if(enabled)		enabled = LockBusy();
			if(enabled)		enabled = OpenLog();

			if(enabled)
			{
				enabled = CheckFolders();
			}
			Log("Avvio programma",false);
		}
		
		/// <summary>
		/// Imposta il delegate alla funzione da eseguire all'avvio
		/// </summary>
		public FuncBkgnd? SetStartFunction()
		{
			FuncBkgnd? f = null;
			switch(cfg.command)
			{
				case 0:
					break;
				case 1:
					par.fmsg("\r\nAVVIO AUTOMATICO: lettura file\r\n");
					f = ReadFile;
					break;
				case 2:
					par.fmsg("\r\nAVVIO AUTOMATICO: scrittura file\r\n");
					f = WriteFile;
					break;
				case 3:
					par.fmsg("\r\nAVVIO AUTOMATICO: lettura e scrittura file\r\n");
					f = ReadWriteFile;
					break;
				default:
					break;

			}
			return f;
		}

		/// <summary>
		/// Rilascia i file aperti (public, chiamata da altro task)
		/// </summary>
		public void ReleaseFiles()
		{
			Log("Fine programma",false);
			ReleaseBusy();
			CloseLog();
		}

		/// <summary>
		/// Verifica la configurazione e crea il lock
		/// </summary>
		/// <returns></returns>
		bool CheckCfg()
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
				iTmp = cfg.maxDepth;

				if(par.usrName.Length < 1)	throw new Exception("Utente non definito");
				
				if(cfg.maxDepth < 0) throw new Exception("maxDepth deve essere nulla o maggiore di zero");

				if(par.fmsg == null) throw new Exception("Puntatatore a funzione fMsg nullo");

				ClearEmptyString(cfg.extYes);
				ClearEmptyString(cfg.extNo);

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
				if(!CheckDir(cfg.logPath, false))
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
					pattern = $"*{cfg.extBusy}";
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

			return ok;
		}

		void ClearEmptyString(List<string> l)
		{
			int i=0;
			Stack<int> stack = new Stack<int>();
			foreach(string s in l)
			{
				if(s.Length == 0)
				{
					stack.Push(i);
				}
				i++;
			}
			while(stack.Count > 0)
			{
				l.RemoveAt(stack.Pop());
			}

		}

		bool CheckFolders()
		{
			bool ok = true;
			
			// Controlla la cartella di origine
			if(!CheckDir(cfg.origRoot, false))
			{
				ok = false;
				MessageBox.Show($"La directory radice di origine {cfg.origRoot} non esiste.");
			}

			// Controlla la cartella di destinazione
			if(!CheckDir(cfg.destRoot, true))
			{
				ok = false;
				MessageBox.Show($"La directory radice di destinazione {cfg.destRoot} non è accessibile in scrittura.");
			}
			return ok;
		}
		/// <summary>
		/// Crea il file di lock
		/// </summary>
		/// <returns></returns>
		bool LockBusy()
		{
			bool ok = false;
			busyFile = cfg.logPath + par.usrName + cfg.extBusy;
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

		/// <summary>
		/// Cancella il file di lock
		/// </summary>
		void ReleaseBusy()
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

		/// <summary>
		/// Controlla se esiste la directory di log
		/// </summary>
		/// <returns></returns>
		bool CheckLogDir()
		{
			bool ok = false;
			ok = Directory.Exists(cfg.logPath);
			return ok;
		}

		/// <summary>
		/// Controlla se la directory esiste
		/// e, se richiesto, se vi si può scrivere.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="write"></param>
		/// <returns></returns>
		bool CheckDir(string path, bool write = false)
		{
			bool ok = true;
			try
			{
				ok = Directory.Exists(path);
			}
			catch (Exception ex)
			{
				ok = false;
				MessageBox.Show($"Cartella '{path}' non trovata.\r\n{ex.Message.ToString()}");		
			}

			if(ok && write)
			{
				try
				{
					string randomfilename;
					do
					{
						randomfilename = path + Path.GetRandomFileName();
					}	while (File.Exists(randomfilename));				// File temporaneo casuale

					StreamWriter sw = File.CreateText(randomfilename);		// Crea in scrittura, chiude e cancella
					sw.Close();
					if(File.Exists(randomfilename))
					{
						File.Delete(randomfilename);
					}

				}
				catch (Exception ex)
				{
					ok = false;
					MessageBox.Show($"Errore di scrittura nella cartelle '{path}'.\r\n{ex.Message.ToString()}");		
				}	
			}

			return ok;
		}

		/// <summary>
		/// Apre il file di log
		/// </summary>
		/// <returns></returns>
		bool OpenLog(bool clear = false)
		{
			bool ok = true;
			logFile = cfg.logPath + cfg.logF;
			if(swLogFile == null)
			{
				string op = string.Empty;
				try
				{
					if((!File.Exists(logFile)) || clear)
					{
						op = "nella creazione";
						swLogFile = File.CreateText(logFile);
					}
					else
					{
						op = "nell'apertura";
						swLogFile = File.AppendText(logFile);
					}
				}
				catch(Exception ex)
				{
					ok = false;
					swLogFile = null;
					MessageBox.Show($"Errore {op} del file di log '{logFile}'\r\n{ex.Message.ToString()}");	
				}
			}
			return ok;
		}

		/// <summary>
		/// Chiude il file di log
		/// </summary>
		void CloseLog()
		{
			if (swLogFile != null)
			{
				swLogFile.Close();
				swLogFile = null;
			}
		}

		/// <summary>
		/// Azzera il file di log
		/// </summary>
		public void ClearLog()
		{
			CloseLog();
			OpenLog(true);	// Clear
		}
		/// <summary>
		/// Aggiunge una linea al log
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="showMsg">Mostra nella finestra</param>
		/// <returns></returns>
		bool Log(string msg, bool showMsg = true)
		{
			bool ok = true;
			if(swLogFile != null)
			{
				try
				{
					swLogFile.WriteLine($"[{DateTime.Now} {par.usrName}] {msg}");
				}
				catch(Exception ex)
				{
					ok = false;
					MessageBox.Show($"Errore nella scrittura del file di log '{logFile}'\r\n{ex.Message.ToString()}");	
				}
			}
			else
			{
				ok = false;
				MessageBox.Show($"Tentativo di scrittura su file di log non aperto");
			}
			if(ok && showMsg)
			{
				par.fmsg(msg);
			}
			return ok;
		}

		/// <summary>
		/// Estrae percorso, nome file ed estensione
		/// </summary>
		/// <param name="fullpath"></param>
		/// <param name="path"></param>
		/// <param name="name"></param>
		/// <param name="ext"></param>
		/// <returns></returns>
		bool DividePath(ref readonly string fullpath, out string path, out string name, out string ext)
		{
			bool ok = false;
			path = name = ext = "";
			if(fullpath != null)
			{
				if(fullpath.Length > 0)
				{
					int iPath;
					iPath = int.Max(fullpath.LastIndexOf('/'),fullpath.LastIndexOf('\\'));		// Inizio del nome del file
					ext = GetExt(in fullpath);
					if(iPath != -1)
					{
						path = fullpath.Substring(0, iPath+1);
					}
					else
					{
						path = string.Empty;
					}
					name = fullpath.Substring(iPath+1,fullpath.Length-ext.Length-path.Length);
					// Path. GetExtension(), GetFileNameWithoutExtension(), GetFullPath() restituiscono solo percorsi Windows con \\
					ok = true;
				}
			}
			return ok;
		}

		/// <summary>
		/// Estrae l'estensione
		/// </summary>
		/// <param name="fullpath"></param>
		/// <returns></returns>
		string GetExt(ref readonly string? fullpath)
		{
			string ext = "";
			if(fullpath != null)
			{
				if(fullpath.Length > 0)
				{
					int iExt = fullpath.LastIndexOf('.');
					if(iExt != -1)
					{
						ext = fullpath.Substring(iExt);
					}
				}
			}
			return ext;
		}

		/// <summary>
		/// Filtra in base all'estensione
		/// </summary>
		/// <param name="fullpath"></param>
		/// <returns></returns>
		public bool FilterExt(ref readonly string fullpath)
		{
			bool yes = false;

			string s = GetExt(in fullpath);
			foreach(string ext in cfg.extYes)
			{
				if(ext == "*")
				{
					yes = true;
					break;
				}
				else
				{
					if(s == ext)
					{
						yes = true;
						break;
					}
				}
			}
		
			foreach(string ext in cfg.extNo)
			{
				if(ext == "*")
				{
					yes = false;
					break;
				}
				else
				{
					if(s == ext)
					{
						yes = false;
						break;
					}
				}
			}
			return yes;
		}


		#warning DA COMPLETARE
		/// <summary>
		/// DA COMPLETARE
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public bool ReadFile(CancellationToken token)
		{
			bool ok = false;

			#warning CREARE PRIMA LE FUNZIONI DI SUPPORTO:
			#warning Confrontare una stringa (estensione) con le estensioni incluse ed escluse.
			#warning Confrontare una stringa (path completo) con le cartelle include ed escluse (\folder\ oppure /folder/)
			#warning Confrontare una stringa (nome con estensione, senza path) con una stringa con caratteri jolly.
			#warning Cercare i file indice.

			#warning DA FARE:
			#warning Aprire il file di log
			#warning Creare una lista
			#warning Cercare i file indice.
			#warning Se ci sono, leggere tutte le righe.
			#warning Se non ci sono file indice, percorrere tutti i file sotto il path di origine.
			#warning Se errore, salvarlo nel file di log.
			#warning Per ogni riga, estrarre l'estensione.
			#warning In base all'estensione, aggiungere la riga alla lista (inserendo il path)
			#warning Verificare se il file esiste.
			#warning Chiudere il file di log con data, ora, utente


			string path, name, ext;
			path = name = ext = string.Empty;

			for(int i = 0;i < 10;i++)       // Esegue le operazioni
				{
					if(token.IsCancellationRequested)
					{
						ok = false;
						break;          // Esce dal ciclo for
					}
					else
					{
						par.fmsg('R'+i.ToString());
						Thread.Sleep(500);
						if(i == 9)
							{
							par.fmsg("\r\n");
							ok = true;
							}
					}
				}
			return ok;
		}

		#warning DA COMPLETARE
		/// <summary>
		/// DA COMPLETARE
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
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
					par.fmsg('W'+i.ToString());
					Thread.Sleep(500);
					if(i == 9)
					{
						par.fmsg("\r\n");
						ok = true;
					}
				}
			}
			return ok;
		}

		#warning DA COMPLETARE
		/// <summary>
		/// DA COMPLETARE
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
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
