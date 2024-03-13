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
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Syncf
{

	public class SyncFile
	{
		public delegate void FuncMsg(string msg, MSG typ);
		public static string STD_CFG = "Syncf.cfg";
		public static string INTERROTTO = "-> INCOMPLETO <-";
		public static int MAXDEPTH = 32756;
		public Color[] txtCol = {Color.Black, Color.Blue, Color.Red};

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
		
		/// <summary>
		/// Cartella di log
		/// </summary>
		public string logPath		{
			get { return cfg.logPath; }
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
				string s;
				enabled = CheckFolders(out s);
				if(!enabled)
				{
					MessageBox.Show(s);
				}
					
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
					par.fmsg("\r\nAVVIO AUTOMATICO: lettura file\r\n",MSG.Message);
					f = ReadFile;
					break;
				case 2:
					par.fmsg("\r\nAVVIO AUTOMATICO: scrittura file\r\n",MSG.Message);
					f = WriteFile;
					break;
				case 3:
					par.fmsg("\r\nAVVIO AUTOMATICO: lettura e scrittura file\r\n",MSG.Message);
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
			bool bTmp;

			// Controlla esistenza delle opzioni necessarie, con assegnazioni fittizie
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
				lsTmp = cfg.matchYes;
				lsTmp = cfg.matchNo ;
				sTmp = cfg.logPath;
				iTmp = cfg.maxDepth;
				sTmp = cfg.redoF;	
				bTmp = cfg.clearLst;

				if(txtCol.Length != (int)MSG.None)	throw new Exception("Color[] txtCol and enum MSG hanno lunghezze differenti");

				if(par.usrName.Length < 1)	throw new Exception("Utente non definito");
				
				if(cfg.maxDepth == -1)	cfg.maxDepth = MAXDEPTH;
				if(cfg.maxDepth < 0) throw new Exception("maxDepth deve essere nulla o maggiore di zero");

				if(par.fmsg == null) throw new Exception("Puntatatore a funzione fMsg nullo");

				ClearEmptyString(cfg.extYes);
				ClearEmptyString(cfg.extNo);
				ClearEmptyString(cfg.matchYes);
				ClearEmptyString(cfg.matchNo);

			}
			catch(Exception ex)
			{
				ok = false;
				MessageBox.Show($"Errore nella configurazione\r\n{ex.Message.ToString()}");
			}

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

		/// <summary>
		/// Elimina le stringhe vuote dalla lista
		/// </summary>
		/// <param name="l"></param>
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

		bool CheckFolders(out string msg)
		{
			bool ok = true;
			
			StringBuilder sb = new StringBuilder();

			// Controlla la cartella di origine
			if(!CheckDir(cfg.origRoot, false))
			{
				ok = false;
				sb.AppendLine($"La directory radice di origine {cfg.origRoot} non esiste.");
			}

			// Controlla la cartella di destinazione
			if(!CheckDir(cfg.destRoot, true))
			{
				ok = false;
				sb.AppendLine($"La directory radice di destinazione {cfg.destRoot} non è accessibile in scrittura.");
			}
			msg = sb.ToString();
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
				par.fmsg($"Errore nella creazione del file '{busyFile}' nella cartella '{cfg.logPath}'\r\n{ex.Message.ToString()}", MSG.Error);
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
					par.fmsg($"Errore nella cancellazione del file '{busyFile}'\r\n{e.Message.ToString()}", MSG.Error);
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
				par.fmsg($"Cartella '{path}' non trovata.\r\n{ex.Message.ToString()}", MSG.Error);
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
					par.fmsg($"Errore di scrittura nella cartelle '{path}'.\r\n{ex.Message.ToString()}", MSG.Error);
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
					par.fmsg($"Errore {op} del file di log '{logFile}'\r\n{ex.Message.ToString()}", MSG.Error);
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
					par.fmsg($"Errore nella scrittura del file di log '{logFile}'\r\n{ex.Message.ToString()}", MSG.Error);	
				}
			}
			else
			{
				ok = false;
				par.fmsg($"Tentativo di scrittura su file di log non aperto", MSG.Error);
			}
			if(ok && showMsg)
			{
				par.fmsg(msg,MSG.Message);
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
		/// Controlla prima extYes, poi extNo
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

		/// <summary>
		/// Filtra in base alle stringhe di confronto
		/// Controlla prima matchYes, poi matchNo
		/// </summary>
		/// <param name="fullpath"></param>
		/// <returns></returns>
		public bool FilterMatch(ref readonly string fullpath)
		{
			bool yes = false;
			foreach(string match in cfg.matchYes)
			{
				string pattern = "^" + Regex.Escape(match).Replace("\\*", ".*").Replace("\\?", ".") + "$";
				if(Regex.IsMatch(fullpath, pattern))
				{
					yes = true;
					break;
				}
			}

			foreach(string match in cfg.matchNo)
			{
				string pattern = "^" + Regex.Escape(match).Replace("\\*", ".*").Replace("\\?", ".") + "$";
				if(Regex.IsMatch(fullpath, pattern))
				{
					yes = false;
					break;
				}
			}

			return yes;
		}

		/// <summary>
		/// Compone la lista dei file indice da leggere
		/// </summary>
		/// <returns></returns>	
		List<string> GetListFiles()
		{	
			List<string> lst = new List<string>();		

			// Syncf -usr <user> -cfg <cfgfile> -lst <lstname> -all
			if(par.fls == FLS.None)			//	Se: <lstname> è nullo, imposta il file <user>.lst e il flag a LST..
			{
				par.lstFile = par.usrName;
				par.fls = FLS.LST;
			}
			if(par.fls == FLS.ALL_LST)		// Se: <lstname> è *, il flag è ALL_LST...
			{
				try
				{
					string[] files = Directory.GetFiles(cfg.logPath);
					foreach(string file in files)
					{
						if(GetExt(in file) == cfg.indxF)
						{
							lst.Add(file);
						}
					}
				}
				catch(Exception ex)
				{
					par.fmsg($"Errore nella ricerca dei file indice in '{logFile}'\r\n{ex.Message.ToString()}", MSG.Error);
				}
			}
			else if(par.fls == FLS.LST)		// Se <lstname> non è nullo (o è stato impostato come <user>, e il flag è LST..
			{
				if(File.Exists(cfg.logPath + par.lstFile + cfg.indxF))
				{
					lst.Add(cfg.logPath + par.lstFile + cfg.indxF);
				}
			}
			return lst.Distinct().ToList();		// Restituisce nuova lista senza duplicati
		}

		/// <summary>
		/// Lista dei file contenuti nei file elencati in ListFile
		/// </summary>
		/// <param name="listFiles"></param>
		/// <param name="token"></param>
		/// <param name="intr"></param>
		/// <returns></returns>
		#warning FUNZIONE DA PROVARE
		List<string> GetFiles(List<string> listFiles, CancellationToken token, out INTERRUZIONE intr)
		{
			List<string> lst = new List<string>();
			StreamReader? sr = null;
			intr = INTERRUZIONE.None;

			try
			{
				foreach(string file in listFiles)
				{				
					if(File.Exists(file))
					{
						string? f;
						sr = new StreamReader(file);
						while ((f = sr.ReadLine()) != null)
						{
							lst.Add(f);
							if(token.IsCancellationRequested)
							{
								throw new WorkingException(INTERROTTO);
							}
						}
						sr.Close();
					}
				}
			}
			catch(Exception ex)
			{
				if(sr != null)
				{
					sr.Close();
				}

				if(ex is WorkingException)
				{
					par.fmsg($"Interrotto", MSG.Warning);
					intr = INTERRUZIONE.TOKEN;
				}
				else
				{
					par.fmsg($"Errore nella lettura dei file indice in '{cfg.logPath}'\r\n{ex.Message.ToString()}", MSG.Error);
					intr = INTERRUZIONE.ERR;
				}

			}
			return lst.Distinct().ToList();
		}


		#warning FUNZIONE DA PROVARE
		List<string> GetFiles(string origRoot, CancellationToken token, out INTERRUZIONE intr)
		{
			

			List<string> lst = new List<string>();
			intr = INTERRUZIONE.None;
			string folder = "";
			Stack<Tuple<string,int>> folders = new Stack<Tuple<string,int>>();
			folders.Push(new Tuple<string,int>(origRoot,0));
			try
			{
				while(folders.Count > 0)
				{
					if(token.IsCancellationRequested)
					{
						throw new WorkingException(INTERROTTO);
					}
					Tuple<string,int> foldDpt = folders.Pop();
					string[] lFiles = Directory.GetFiles(foldDpt.Item1);
					string[] lFolders = Directory.GetDirectories(foldDpt.Item1);
					if(foldDpt.Item2 <= cfg.maxDepth)
					{
						foreach(string lFile in lFiles)
						{
							if(FilterExt(lFile))
							{
								if(FilterMatch(lFile))
								{
									lst.Add(lFile);
								}
							}
						}
						foreach(string lFolder in lFolders)
						{
							folders.Push(new Tuple<string,int>(lFolder,foldDpt.Item2+1));
						}
					}
				}
			}
			catch(Exception ex)
			{
				if(ex is WorkingException)
				{
					par.fmsg($"Interrotto", MSG.Warning);
					intr = INTERRUZIONE.TOKEN;
				}
				else
				{
					par.fmsg($"Errore durante la lettura della cartella '{folder}'\r\n{ex.Message.ToString()}", MSG.Error);
					intr = INTERRUZIONE.ERR;
				}

			}

			return lst.Distinct().ToList();
		}


		#warning Aggiungere cfg: cancella i file indice dopo l'uso
		#warning Aggiungere cfg: file indice 'non copiati'
		#warning Modificare cfg: origRoot e destRoot devono essere array della stessa dimensione
		#warning Aggiungere Log() nei punti significativi

		/// <summary>
		/// DA COMPLETARE
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public bool ReadFile(CancellationToken token)
		{
			bool ok = false;
			List<string> files;
			INTERRUZIONE intr;

			#warning Aprire il file di log.
			#warning Salvare operazioni ed errori nel file di log.

			List<string> lstFiles = GetListFiles();

			#warning ELIMINARE DOPO TEST
			lstFiles.Clear();

			if(lstFiles.Count > 0)
			{
				//Log(
				files = GetFiles(lstFiles, token, out intr);	
			}
			else
			{
				files = GetFiles(cfg.origRoot, token, out intr);
			}

			#warning Chiudere il file di log con data, ora, utente
			

			StringBuilder sb = new StringBuilder();
			foreach (string s in files)
			{
				sb.AppendLine(s);
			}
			//MessageBox.Show(sb.ToString());
			#warning Al file todo.txt vanno aggiunte linee con AppendLine()
			#warning Correggere dopo test
			File.WriteAllText(cfg.logPath + cfg.todoF, sb.ToString());

			#if false
			string path, name, ext;
			path = name = ext = string.Empty;

			for(int i = 0;i < 30;i++)       // Esegue le operazioni
				{
					if(token.IsCancellationRequested)
					{
						ok = false;
						break;          // Esce dal ciclo for
					}
					else
					{
						par.fmsg('R'+i.ToString(),MSG.Message);
						Thread.Sleep(500);
						if(i == 29)
							{
							par.fmsg("\r\n",MSG.Message);
							ok = true;
							}
					}
				}
			#endif

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
					par.fmsg('W'+i.ToString(),MSG.Message);
					Thread.Sleep(500);
					if(i == 9)
					{
						par.fmsg("\r\n",MSG.Message);
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
