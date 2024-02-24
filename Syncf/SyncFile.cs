using Fred68.CfgReader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Syncf
{
	public class SyncFile
	{

		string cfgName = "Syncf.cfg";
		dynamic cfg;
		string userName = string.Empty;
		bool enabled;
		
		public bool isEnabled
		{
			get { return enabled; }
		}

		/// <summary>
		/// Ctor
		/// </summary>
		public SyncFile()
		{
			cfg = new CfgReader();
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
			string tmp;
			List<string> lstmp;

			// Controlla esistenza delle opzioni necessarie
			try
			{	
				tmp = cfg.busyF;
				tmp = cfg.busyF;
				tmp = cfg.logF;
				tmp = cfg.todoF;
				tmp = cfg.doneF;
				tmp = cfg.indxF;
				tmp = cfg.origRoot;
				tmp = cfg.destRoot;
				lstmp = cfg.ext;
				tmp = cfg.logPath;
				if(userName.Length < 1)	throw new Exception("Utente non definito");
					
			}
			catch(Exception ex)
			{
				ok = false;
				MessageBox.Show($"Errore nella configurazione\r\n{ex.Message.ToString()}");
			}
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
						Thread.Sleep(500);
						if(i == 9)
							{
							ok = true;
							}
					}
				}
			return ok;
		}
	}
}
