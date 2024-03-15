using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Syncf.SyncFile;

namespace Syncf
{
	public enum INTERRUZIONE { ERR, TOKEN, None };

	public enum MSG { Message, Warning, Error, None };

	public class WorkingException : Exception
	{
		public WorkingException()	{}
		public WorkingException(string message) : base(message)	{}

		public WorkingException(string message, Exception inner) : base(message, inner)	{}
	}

	public delegate bool FuncBkgnd(CancellationToken tk);

	public enum FLS { LST, ALL_LST, ALL, None };

	public class SyncfParams
	{
		public FuncMsg fmsg;			// Delegate funzione che mostra i messaggi (altro task)
		public string usrName;			// Nome utente
		public string cfgFile;			// File di configurazione
		public string lstFile;			// Nome (senza estensione) del fine con la lista
		public FLS fls;					// Liste da utilizzate
		public bool noFilterLst;		// Non applica i filtri alle lste

		public SyncfParams()
		{
			this.fmsg = (string s,MSG t,int n) => {};	// Delegate che non fa nulla
			this.usrName = this.cfgFile	= cfgFile = this.lstFile = string.Empty;
			this.fls = FLS.None;
			this.noFilterLst = false;
		}
		public SyncfParams(FuncMsg f,string usrName, string cfgFile, string lstFile, FLS fls, bool noFilterLst)
		{
			this.fmsg		= f;
			this.usrName	= usrName;
			this.cfgFile	= cfgFile;
			this.lstFile	= lstFile;
			this.fls		= fls;
			this.noFilterLst = noFilterLst;
		}
	}

}
