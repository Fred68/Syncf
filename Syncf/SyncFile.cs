using Fred68.CfgReader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Syncf
{
	public class SyncFile
	{

		string cfgName = "Syncf.cfg";
		dynamic cfg;

		string userName;
		public SyncFile()
		{
			cfg = new CfgReader();
			cfg.ReadConfiguration(cfgName);
			userName = Environment.UserName;			// System.Security.Principal.WindowsIdentity.GetCurrent().Name
		}
		
		public string CfgMsgs()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(cfg.ToString());
			sb.AppendLine(cfg.DumpEntries());
			sb.AppendLine($"User: {userName}");
			return sb.ToString();
		}

	}
}
