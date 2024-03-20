
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Fred68.CfgReader;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Drawing.Interop;


namespace Syncf
{

	public class SyncFile
	{
		public delegate void FuncMsg(string msg, MSG typ, int newlines = 1);
		public static string STD_CFG = "Syncf.cfg";
		public static string INTERROTTO = "-> INCOMPLETO <-";
		public static int MAXDEPTH = 1024;
		public static string TMP = ".tmp";
		public Color[] txtCol = {Color.Black, Color.Blue, Color.Red};

		SyncfParams par;
		dynamic cfg;
		string stdUserName;
		bool enabled = false;				// true se non ci sono errori di configurazione
		string busyFile = string.Empty;		// File busy
		string logFile = string.Empty;		// File di log
		string msgReadText = string.Empty;
		StreamWriter? swLogFile = null;		// Streamwriter (oppure null)

		class filePair
		{
			string orig {get; set;}
			string dest {get; set;}
			bool move {get; set;}	
			public filePair()
			{
				orig = dest = string.Empty;
				move = false;
			}
			public filePair(string origin, string destination, bool removeOrig = false)
			{
				orig = origin;
				dest = destination;
				move = removeOrig;
			}
			public override string ToString()
			{
				string s = move ? " -> spostato" : "";
				return $"{orig}; {dest};{s}";
			}
		}

		#region PROPRIETA'
		
		/// <summary>
		/// Comandi abilitati, nessun errore d configurazione
		/// </summary>
		public bool IsEnabled
		{
			get { return enabled; }
		}

		/// <summary>
		/// Delegate per visualizzare i messaggi del task principale
		/// </summary>
		public FuncMsg? FuncMessage
		{
			get {return par.fmsg;}
		}

		/// <summary>
		/// Messaggio con la configurazione
		/// </summary>
		public string MsgConfiguration
		{
			get
			{
				StringBuilder sb = new StringBuilder();
				sb.AppendLine($"Cfg filename = {par.cfgFile}");
				sb.AppendLine($"User: {par.usrName} [{stdUserName}]");
				sb.AppendLine(msgReadText);

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
		public string LogPath
		{
			get { return cfg.logPath; }
		}

		/// <summary>
		/// File di log
		/// </summary>
		public string LogFile
		{
			get { return logFile; }
		}

		/// <summary>
		/// File da copiare
		/// </summary>
		public string TodoFile
		{
			get { return cfg.logPath + cfg.todoF; }
		}

		/// <summary>
		/// File da copiare non trovati
		/// </summary>
		public string MissFile
		{
			get { return cfg.logPath + cfg.missF; }
		}

		/// <summary>
		/// File da copiare non copiati perché con modifiche meno recenti della destinazione
		/// </summary>
		public string OldFile
		{
			get { return cfg.logPath + cfg.oldF; }
		}

		/// <summary>
		/// File copiati (origine; destinazione; flag)
		/// </summary>
		public string DoneFile
		{
			get { return cfg.logPath + cfg.doneF; }
		}
		#endregion

		/// <summary>
		/// Ctor
		/// Apre subito il file di log
		/// </summary>	
		public SyncFile(SyncfParams p)
		{
			cfg = new CfgReader();			// Attenzione a cfg.CHR_Ammessi
			par = new SyncfParams();
			
			par.cfgFile = STD_CFG;
			par.fmsg = p.fmsg;
			par.usrName = stdUserName = par.lstFile = string.Empty;
			par.fls = p.fls;

			if(p.cfgFile.Length > 2)	par.cfgFile = p.cfgFile;
			cfg.ReadConfiguration(par.cfgFile);
			
			#warning Impostare cfg.noWrite in base al valore letto e a par.noWrite degli argomenti

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
			else if(par.fls == FLS.None)				// Se non è specificato alcun file di lista...
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

			if(enabled)
			{
				switch(par.fls)
				{
					case FLS.ALL:
					{
						msgReadText = $"Cerca tutti i file in '{cfg.origRoot}'";
					}
					break;
					case FLS.ALL_LST:
					{
						msgReadText = $"Legge tutti i file indice '{cfg.indxF}' in '{cfg.logPath}'";
					}
					break;
					case FLS.LST:
					{
						msgReadText = $"Legge il file indice '{par.lstFile}{cfg.indxF}' in '{cfg.logPath}'";
					}
					break;
				}
			}

			Log("Avvio programma", false);
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
					par.fmsg("\r\nAVVIO AUTOMATICO: lettura file",MSG.Message);
					f = ReadFile;
					break;
				case 2:
					par.fmsg("\r\nAVVIO AUTOMATICO: scrittura file",MSG.Message);
					f = WriteFile;
					break;
				case 3:
					par.fmsg("\r\nAVVIO AUTOMATICO: lettura e scrittura file",MSG.Message);
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
				sTmp = cfg.oldF;	
				bTmp = cfg.clearLst;
				bTmp = cfg.delOrig;
				sTmp = cfg.missF;
				bTmp = cfg.noWrite;

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

		public bool Log(string msg, bool showMsg = true)
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
					par.fmsg($"Errore nella scrittura del file di log '{logFile}'\r\n{ex.Message}", MSG.Error);	
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
		public string GetExt(ref readonly string? fullpath)
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

		bool GetDestinationFileName(ref readonly string fullpath, out String destPath)
		{
			bool ok = false;	
			destPath = "";
			if((fullpath != null) && (fullpath != string.Empty))
			{
				int pos = fullpath.IndexOf(cfg.origRoot,StringComparison.InvariantCultureIgnoreCase);
				if(pos == 0)
				{
					destPath = cfg.destRoot + fullpath.Substring(cfg.origRoot.Length);
					ok = true;
				}
				else if(pos == -1)
				{
					par.fmsg($"Path non trovato in '{fullpath}'", MSG.Error);
					Log($"Errore: path '{cfg.origRoot}' non trovato in '{fullpath}'", false);
				}
				else
				{
					par.fmsg($"Path non all'inizio di '{fullpath}'", MSG.Error);
					Log($"Errore: path '{cfg.origRoot}' non all'inizio di '{fullpath}'", false);

				}
			}
			return ok;
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

			if(par.fls == FLS.None)			//	Se: <lstname> è nullo... 
			{
				if(File.Exists(cfg.logPath + par.usrName + cfg.indxF))		// ...ed esiste il file <user>.lst...
				{
					par.lstFile = par.usrName;		// ...imposta il file <user>.lst e il flag a LST.
					par.fls = FLS.LST;	
				}
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
		/// Non applica filtri
		/// </summary>
		/// <param name="listFiles"></param>
		/// <param name="token"></param>
		/// <param name="intr"></param>
		/// <returns></returns>
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
						Log($"Lettura file indice: {file}",true);
						while ((f = sr.ReadLine()) != null)
						{
							Thread.Sleep(1);	// Per catturare il token
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

		/// <summary>
		/// Lista dei file in una cartella e nelle sottocartelle
		/// Applica i filtri
		/// </summary>
		/// <param name="origRoot"></param>
		/// <param name="token"></param>
		/// <param name="intr"></param>
		/// <returns></returns>
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
					Tuple<string,int> foldDpt = folders.Pop();
					Log($"Lettura cartella: {foldDpt.Item1}",true);
					string[] lFiles = Directory.GetFiles(foldDpt.Item1);
					string[] lFolders = Directory.GetDirectories(foldDpt.Item1);
					if(foldDpt.Item2 <= cfg.maxDepth)
					{
						foreach(string lFile in lFiles)
						{
							Thread.Sleep(1);	// Per catturare il token
							if(FilterExt(in lFile))
							{
								if(FilterMatch(in lFile))
								{
									lst.Add(lFile);
								}
							}
							if(token.IsCancellationRequested)
							{
								throw new WorkingException(INTERROTTO);
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

		#warning Nota: per cartelle multiple, usare file .cfg distinti.
		#warning Aggiungere Log() nei punti significativi
		#warning Svuotare file .lst. se richiesto
		#warning Argomento ed opzione nowrite: wsegue tutto, ma senza scrivere i file di destinazione
	
		///// <summary>
		///// Rimuove le righe duplicate in file
		///// </summary>
		///// <param name="file"></param>
		///// <param name="token"></param>
		///// <param name="intr"></param>
		///// <returns></returns>
		///// 

		/// <summary>
		/// Rimuove le righe duplicate in file
		/// Sovrascrive il file originario, se richiesto...
		/// ...oppure crea file temporaneo (e mette il nome in tmp)
		/// </summary>
		/// <param name="file"></param>
		/// <param name="overwrite">sovrascrive 'file', se false crea</param>
		/// <param name="token"></param>
		/// <param name="intr"></param>
		/// <returns></returns>
		bool RemoveDuplicatesFromTxtFile(string file, bool overwrite, CancellationToken token, out INTERRUZIONE intr, out string tmp)
		{
			bool ok = false;
			intr = INTERRUZIONE.None;
			StreamReader? sr = null;
			List<string> lst = new List<string>();
			string path, name, ext;
			tmp = "";

			try
			{
				if(File.Exists(file) && DividePath(ref file, out path, out name, out ext))
				{
					string? f;
					sr = new StreamReader(file);
					while ((f = sr.ReadLine()) != null)
					{
						Thread.Sleep(1);	// Per catturare il token
						lst.Add(f);
						if(token.IsCancellationRequested)
						{
							throw new WorkingException(INTERROTTO);
						}
					}
					sr.Close();

					tmp = path + name + TMP;

					lst = lst.Distinct().ToList();
					File.WriteAllLines(tmp, lst);
					
					if(overwrite)
					{
						File.Delete(file);
						File.Move(tmp, file);
					}
					Log($"Eliminati duplicati da {file}", true);
					ok = true;
				}
			}
			catch(Exception ex)
			{
				if(ex is WorkingException)
				{
					par.fmsg($"Operazione interrotta durante la rimozione dei duplicati in '{file}'", MSG.Warning);
					Log($"Interrotta rimozione duplicati in {file}", false);
					intr = INTERRUZIONE.TOKEN;
				}
				else
				{
					par.fmsg($"Errore durante la rimozione dei duplicati in '{file}'\r\n{ex.Message.ToString()}", MSG.Error);
					Log($"Errore durante la rimozione dei duplicati in '{file}'", false);
					intr = INTERRUZIONE.ERR;
				}
			}
			finally
			{
				if(sr != null)
					{
						sr.Close();
					}
			}
			return ok;
		}

		/// <summary>
		/// DA COMPLETARE
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public bool ReadFile(CancellationToken token)
		{
			bool ok = false;
			List<string>? files = null;
			INTERRUZIONE intr = INTERRUZIONE.None;

			List<string> lstFiles = GetListFiles();

			if(lstFiles.Count > 0)
			{
				files = GetFiles(lstFiles, token, out intr);
				ok = true;
			}
			else if(par.fls == FLS.ALL)
			{
				files = GetFiles(cfg.origRoot, token, out intr);
				ok = true;
			}

			if(files == null)
			{
				ok = false;
				Log("Nessun file da leggere richiesto");
			}

			switch(intr)
			{
				case INTERRUZIONE.ERR:
				{
					Log("Errore durante la lettura dei file");
				}
				break;
				case INTERRUZIONE.TOKEN:
				{
					Log("Lettura dei file interrotta");
				}
				break;
				default:
				{
					if(ok)	Log("Lettura dei file completata");
				}
				break;
			}

			if(ok && (files != null))
			{
				try
				{
					File.AppendAllLines(TodoFile, files);
					Log($"Aggiornato {TodoFile}");
				}
				catch (Exception ex)
				{
					ok = false;
					par.fmsg($"Errore nella lettura dei file indice in '{cfg.logPath}'\r\n{ex.Message.ToString()}", MSG.Error);
					Log($"Errore durante la scrittura su: {TodoFile}", false);		
				}
			}

			return ok;
		}

		/// <summary>
		/// Rimuove i duplicati dal file todoF
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public bool RemoveTodoDuplicates(CancellationToken token)
		{
			bool ok = false;
			INTERRUZIONE intr = INTERRUZIONE.None;
			string tmp;
			ok = RemoveDuplicatesFromTxtFile(TodoFile, true, token, out intr, out tmp);

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
			INTERRUZIONE intr = INTERRUZIONE.None;

			Stack<string> todo;			// File di origine da copiare
			List<string> miss;			// File di origine da copiare non trovati
			List<filePair> done;		// File di origine e destinazione copiati + flag moved
			List<string> old;			// File di origine più vecchi dei file di destinazione
			List<string> skp;			// File di todo non copiati
			string tmpFile;				// File temporaneo con la lista

			
			todo = new Stack<string>();
			miss = new List<string>();
			done = new List<filePair>();
			old = new List<string>();
			skp = new List<string>();

			ok = RemoveDuplicatesFromTxtFile(TodoFile, false, token, out intr, out tmpFile);	// Copia Todofile in tmpFile, rimuovendo i duplicati


			if(ok)		// Legge tutti i file di todo.tmp e li aggiunge allo stack todo. Se errore: esce dal ciclo.
			{
				StreamReader? sr = null;
				try
				{
					sr = new StreamReader(tmpFile);
					string? f;
					while ((f = sr.ReadLine()) != null)
					{
						Thread.Sleep(1);	// Per catturare il token
						todo.Push(f);
						if(token.IsCancellationRequested)
						{
							throw new WorkingException(INTERROTTO);
						}
					}
				}
				catch (Exception ex)
				{
					if(ex is WorkingException)
					{
						ok = false;
						par.fmsg($"Operazione interrotta durante la lettura del file '{tmpFile}'", MSG.Warning);
						Log($"Interrotta lettura del '{tmpFile}'", false);
						intr = INTERRUZIONE.TOKEN;
					}
					else
					{
						ok = false;
						par.fmsg($"Errore durante la lettura del file '{tmpFile}'\r\n{ex.Message.ToString()}", MSG.Error);
						Log($"Errore durante la lettura del file '{tmpFile}'", false);
						intr = INTERRUZIONE.ERR;
					}
				}
				finally
				{
					if(sr != null)
					{
						sr.Close();
					}
					if(!ok)
					{
						todo.Clear();	// Se la lettura non è stata completata, il file todo originario resta invariato
					}
				}
			}
			
			if(ok)		// Estrae tutti i file di todo e lo copia nelle cartelle di destinazione
			{
				
				while(todo.Count > 0)
				{
					string origFile = todo.Pop();		// Nome del file di origine
					string destFile;
					FileInfo origNfo, detNfo;
					bool bWrite = false;
					Thread.Sleep(1);					// Cattura eventuale token
					if(token.IsCancellationRequested)
					{
						throw new WorkingException(INTERROTTO);
					}
					try
					{
						if(GetDestinationFileName(ref origFile, out destFile))
						{
							if(File.Exists(origFile))
							{
								origNfo = new FileInfo(origFile);
								if(File.Exists(destFile))
								{
									detNfo = new FileInfo(origFile);
									if(detNfo.LastWriteTime < origNfo.LastWriteTime)
									{
										bWrite = true;
									}
									else
									{
										old.Add(origFile);		// Il file di destinazione è più recente del file di origine
									}
								}
								else
								{
									bWrite = true;
								}
								if(bWrite)
								{
									string path, name, ext;
									if(DividePath(ref destFile, out path, out name, out ext))
									{
										#warning Non eseguire alcuna scrittura sul path di destinazione se cfg.noWrite è true, nè cancellare origFile.
										Directory.CreateDirectory(path);
										File.Copy(origFile, destFile);
										if(cfg.delOrig)
										{
											File.Delete(origFile);
										}
										done.Add(new filePair(origFile, destFile, cfg.delOrig));
									}
									else
									{
										throw new Exception($"Errore nell'estrazione della cartella di '{destFile}'");
									}
								}
							}
							else
							{
								miss.Add(origFile);		// File di origine inesistente
							}
						}
					}
					catch (Exception ex)
					{
						skp.Add(origFile);				// Aggiunge il file attuale, non copiato, alla lista
						if(ex is WorkingException)
						{
							todo.Clear();				// Interrompe tutte le operazioni, svuotando la lista
							ok = false;
							par.fmsg($"Operazione interrotta durante la copia del file '{origFile}'", MSG.Warning);
							Log($"Interrotta copia del file '{origFile}'", false);
							intr = INTERRUZIONE.TOKEN;
						}
						else
						{								// Errore, ma continua il ciclo while
							ok = false;
							par.fmsg($"Errore durante la copia del file '{origFile}'\r\n{ex.Message.ToString()}", MSG.Error);
							Log($"Errore durante la copia del file '{origFile}'", false);
							intr = INTERRUZIONE.ERR;
							continue;
						}
					}
				}   // fine while(...)
			}   // fine if(...)

			skp.AddRange(todo);				// File la cui copia non è stata eseguita (errore o interruzione)

			try								// Aggiorna i file con le liste, anche se c'é stato un errore
			{
				WriteListToFile<string>(miss, MissFile, true);	
				WriteListToFile<string>(old, OldFile, true);
				WriteListToFile<filePair>(done, DoneFile, true);

				if(skp.Count > 0)			// Se ci sono elementi non copiati, li sovrascrive alla lista originaria
				{
					WriteListToFile<string>(skp, TodoFile, false);	
				}

				if(File.Exists(tmpFile))
				{
					File.Delete(tmpFile);
				}
				
			}
			catch (Exception ex)
			{
				par.fmsg($"Errore durante l'aggiornamento delle liste'\r\n{ex.Message.ToString()}", MSG.Error);
				Log($"Errore durante l'aggiornamento delle liste", false);
			}

			switch(intr)
			{
				case INTERRUZIONE.ERR:
				{
					Log("Errore durante la copia dei file");
				}
				break;
				case INTERRUZIONE.TOKEN:
				{
					Log("Copia dei file interrotta");
				}
				break;
				default:
				{
					if(ok)	Log("Copia dei file completata");
				}
				break;
			}

			return ok;
		}

		bool WriteListToFile<T>(List<T> list, string file, bool append)
		{
			bool ok = true;
			StreamWriter? sw = null;
			
			try
			{
				sw = new StreamWriter(file, append);	// Append
				foreach(T el in list)
				{
					sw.WriteLine(el.ToString());
				}
			}
			catch (Exception ex)
			{
				par.fmsg($"Errore durante scritture del file '{file}'\r\n{ex.Message.ToString()}", MSG.Error);
				Log($"Errore durante scritture del file '{file}", false);	
				ok = false;
			}
			return ok;
		}

		#warning DA CONTROLLARE
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
