﻿
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

		string cfgName = "Syncf.cfg";
		dynamic cfg;
		string userName = string.Empty;
		string altUserName = string.Empty;
		bool enabled = false;				// true se non ci sono errori di configurazione
		FuncMsg fmsg;						// delegate per visualizzare i messaggi del task principale
		string busyFile = string.Empty;		// File busy
		string logFile = string.Empty;		// File di log
		StreamWriter? swLogFile = null;		// File di log (oppure null)


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
		public FuncMsg funcMessage
		{
			get {return fmsg;}
		}

		/// <summary>
		/// Messaggio con la configurazione
		/// </summary>
		public string msgConfiguration
		{
			get
			{
				StringBuilder sb = new StringBuilder();
				sb.Append($"Cfg filename = {cfgName}");
				sb.AppendLine(cfg.ToString());
				sb.AppendLine(cfg.DumpEntries());
				sb.AppendLine($"User: {userName}");
				if(altUserName.Length > 0)	sb.AppendLine($"AltUser: {altUserName}");
				return sb.ToString();
			}
		}

		#endregion


		/// <summary>
		/// Ctor
		/// </summary>
		public SyncFile(FuncMsg f,string usrName, string cfgFile)
		{
			cfg = new CfgReader();
			fmsg = f;
			#if DEBUG
			//MessageBox.Show(cfg.CHR_Ammessi);
			#endif
			if(cfgFile.Length > 2)	cfgName = cfgFile;
			cfg.ReadConfiguration(cfgName);
			userName = Environment.UserName;			// System.Security.Principal.WindowsIdentity.GetCurrent().Name
			if(usrName.Length > 1)		altUserName = usrName;
			enabled = CheckCfg();
			if(enabled)		enabled = LockBusy();
			if(enabled)		enabled = OpenLog();
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

			

			return ok;
		}

		/// <summary>
		/// Crea il file di lock
		/// </summary>
		/// <returns></returns>
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
					swLogFile.WriteLine($"[{DateTime.Now} {userName}] {msg}");
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
				fmsg(msg);
			}
			return ok;
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
			#warning Estrarre l'estensione di una stringa, il nome file ed il path.
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
			#warning CREARE PRIMA LE FUNZIONI DI SUPPORTO:
			#warning Per ogni riga, estrarre l'estensione.
			#warning In base all'estensione, aggiungere la riga alla lista (inserendo il path)
			#warning Verificare se il file esiste.
			#warning Chiudere il file di log con data, ora, utente





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
