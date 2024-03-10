using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// using System.Threading.Tasks;

using System.IO;                            // Operazioni su file
using System.Text.RegularExpressions;		// Regex
using StringExtension;                      // Funzioni extra
using Fred68.GenDictionary;                 // Dizionario generico
using System.Dynamic;						// Per usare dynamic e TryGetMember
using System.Reflection;					// Reflection (per accedere ai membri pubblici di una classe derivata)


namespace Fred68.CfgReader
	{

	#warning Completare DATE
	#warning Aggiungere uso delle variabili (concatenazione, voce singola, somma, differenza)
	

	/// <summary>
	/// Config text file reader
	/// </summary>
	public partial class CfgReader : DynamicObject
		{

		StringBuilder _msg;
		bool ok;
		List<string> _lines;								// Righe valide del file
		Dictionary<string, bool> _sect;						// Sezioni attive o disattive
		GenDictionary.GenDictionary _dict;					// Dizionario generico
		Dictionary<string, Func<string, bool>> _cmds;		// Dizionario dei comandi

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="filename">Config file name</param>
		public CfgReader()
			{
			_lines = new List<string>();
			_sect = new Dictionary<string, bool>();
			_dict = new GenDictionary.GenDictionary();
			_msg = new StringBuilder();
			_cmds = new Dictionary<string, Func<string, bool>>();
			Clear();
			CreateCommands();
			}

		public void ReadConfiguration(string filename)
			{
			ReadConfig(filename);
			MergeLines();
			Process();
			}

		/// <summary>
		/// Azzera
		/// </summary>
		public void Clear()
			{
			_lines.Clear();
			_sect.Clear();
			_dict.Clear();
			_msg.Clear();
			ok = true;
			}

		/// <summary>
		/// ToString() override
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(_msg.ToString());							// Messaggi
			sb.AppendLine(IsOk ?									// stato
							MSG.LetturaConfigurazioneOK :
							MSG.LetturaConfigurazioneERR);
			return sb.ToString();
			}

		/// <summary>
		/// Dump delle variabili
		/// </summary>
		/// <returns></returns>
		public string DumpEntries()
			{
			return _dict.Dump();
			}
		
		/// <summary>
		/// Dump delle linee lette
		/// </summary>
		/// <returns></returns>
		public string DumpLines()
			{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(string.Format(CfgReader.MSG.LineeValide,_lines.Count));
			int i = 1;							// Parte da linea 1
			foreach(string s in _lines)			// Linee
				{
				sb.AppendLine($"{(i.ToString()+':').PadRight(PADlines, ' ')}{s}");
				i++;
				}
			return sb.ToString();
			}

		/// <summary>
		/// Legge il file di testo e inserisce le linee valide nella lista
		/// </summary>
		/// <param name="filename"></param>
		void ReadConfig(string filename)
			{
			try
				{
				string line;
				string fn = Path.GetFullPath(filename);
				if(!File.Exists(fn))
					{
					throw new Exception(string.Format(MSG.FileNotFound, fn));				
					}
				using (StreamReader reader = new StreamReader(fn))
					{
					_lines.Clear();
					int n = 0;									// Numero di riga del file
					while ((line = reader.ReadLine()) != null)
						{
						n++;
						line = RemoveComment(line);
						#if false
						if(line.Length > 0)
							{
							
							if(line.StartsWith(STR_Errore))
								throw new Exception(string.Format(MSG.ErroreNellaRiga, n, line));
							
							}
						#endif
						_lines.Add(line);					// Linea aggiunta anch se vuota, per mantere il conteggio
						}
					}
				}
			catch (Exception ex)
				{
				_msg.AppendLine(ex.Message);
				ok = false;
				}
			}

		/// <summary>
		/// Elimina li linee vuote
		/// </summary>
		void ClearEmptyLines()
		{
			List<string> lines = new List<string>();
			
			foreach(string line in _lines)
			{
				if(line.Length > 0)
				{
				lines.Add(line);
				}
			}
			_lines.Clear();
			_lines = lines;
		}

		/// <summary>
		/// Elimina il separatore di linea ed unisce le linee senza separatore alla successiva
		/// </summary>
		void MergeLines()
		{
			List<string> lines = new List<string>();
			ClearEmptyLines();
			string prevLine = "";
			for(int i=0; i < _lines.Count; i++)
			{
				string s = _lines[i];	
				List<int> indxs = s.IndexOfOutside(CHR_MergeNextLine,CHR_StringDelimiter,CHR_StringDelimiter);
				if(indxs.Count > 0)
				{
					prevLine += s.Substring(0,indxs[indxs.Count-1]);
				}
				else
				{
					lines.Add(prevLine + s);
					prevLine = "";
				}
			}
			_lines.Clear();
			_lines = lines;
		return;
		}

		/// <summary>
		/// Elimina i commenti e i caratteri non ammessi
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		string RemoveComment(string line)
			{
			string s = string.Empty;
			if( (line.Length > 0) && (!line.StartsWith(CHR_Commento)))		// Se linea vuota o inizia con carattere di commento: lascia la stringa vuota.
				{
				
				#if false		// DISABILITATO per usare altro metodo
				int i1,i2;
				i1 = int.MaxValue; i2 = int.MinValue;						// Prima cerca una eventuale stringa che potrebbe contenere il commento
				Match m = Regex.Match(line, CHR_StringDelimiterRgx);		
				if(m.Success)
					{
					i1 = m.Index;
					i2 = m.Index + m.Length;
					}

				int i, start;												// Cerca il primo indice di commento non incluso nella stringa
				start = 0;
				do
					{
					i = line.IndexOf(CHR_Commento, start);					// Cerca
					if( i != -1)											// Aggiorna il punto di partenza
						start = i+1;
					}
				while( (i != -1) && ((i >= i1) && (i <= i2)) );				// ...ripete finché è compreso nella eventuale stringa
				#endif

				#if true
				List<int> indxs = line.IndexOfOutside(CHR_Commento,CHR_StringDelimiter,CHR_StringDelimiter);
				int i = -1;
				if(indxs.Count > 0)		i = indxs[0];
				#endif
				
				if( i == -1)												// Se non ha trovato il carattere di commento...
					{
					s = line;												// Restituisce la stringa
					}
				else
					{														// Se ha trovato il carattere di commento...
					s = line.Substring(0, i);								// Elimina dal carattere in poi (compreso)
					}
				}
			s = s.Trim();													// Elimina gli spazi iniziali e finali
			s = Regex.Replace(s, CHR_Ammessi, "");							// Elimina i caratteri non ammessi
			return s;
			}

		/// <summary>
		/// Esamina il contenuto delle linee valide
		/// </summary>
		void Process()
			{
			string sect;
			Tuple<string, string> tcom,t;
			string section = string.Empty;

			int linenum = 1;											// Contatore
			bool continua = true;										// Flag per uscire prima dal ciclo
			bool sintassiAttiva = true;									// Si ferma se c'è un errore di sintassi

			foreach(string line in _lines)								// foreach fuori dal try: elenca tutte le eccezioni senza fermarsi
				{
				if(!continua)											// Esce dal ciclo, se richiesto
					break;	
				try
					{

					sect = IdentifySection(line);						// Identifica una sezione  (stringa vuota se non trovato)
					tcom = IdentifyCommand(line);						// Identifica un comando
					t = IdentifyVariable(line);							// Identifica una assegnazione di variabile

					if(	(line.Length > 0) &&				// linea non vuota
						(sect.Length == 0) &&				// linea senza stringa di sezione 
						(tcom.Item1.Length == 0) &&			// Comando vuoto
						(t.Item1.Length == 0) &&			// Nome della variabile vuoto
						(_sect[section]) &&					// Sezione corrente attiva
						sintassiAttiva)						// Sintassi attiva
						{
						throw new Exception("Errore di sintassi");	// ...genera un'eccezione.
						}

					if(sect.Length > 0)									// Se ha identificato una sezione
						{
						if(sect == CHR_SezioneEnd)						// Se fine sezione: reimposta stringa vuota come sezione attiva
							{
							section = string.Empty;
							}
						else
							{
							section = sect;								// Se no: imposta il nome come sezione corrente (attiva)
							if(!_sect.ContainsKey(section))				// Se non è ancora presente trale sezioni...
								{
								_sect[section] = true;					// ...la aggiunge e la attiva (di default)
								}
							}
						}
					
					if(tcom.Item1.Length > 0)							// Esegue il comando...
						{
						if( (section.Length==0) || ((section.Length>0) && (_sect[section])))	// Se fuori sezione o in una sezione attiva
							{
							sintassiAttiva = true;
							continua = _cmds[tcom.Item1](tcom.Item2);	// Chiama la funzione
							}
						else
							{
							sintassiAttiva = false;
							}
						}												// ...oppure esegue l'assegnazione:
					else if( ((sect = IdentifySection(t.Item1)).Length > 0) && (sect != CHR_SezioneEnd) )	// Se identifica un nome di sezione nel nome...
						{												// ...della variabile dell'assegnazione, analizza la sezione.
						if(!_sect.ContainsKey(sect))					// Se il nome di sezione non è ancora presente tra le sezioni...
							{											// ...non è stato riconosciuto nelle precedenti istruzioni:...
							throw new Exception(string.Format(MSG.SezioneNonRiconosciuta, sect));	// ...genera un'eccezione.
							}
						else
							{
							if(t.Item2 == STR_On)						// Se è ON, la attiva...
								{
								_sect[sect] = true;
								section = string.Empty;					// ...e azzera il nome della sezione corrente, identificato prima.
								}
							else if(t.Item2 == STR_Off)					// Se è OFF, la disattiva...			
								{
								_sect[sect] = false;
								section = string.Empty;					// ...e azzera il nome della sezione corrente, identificato prima.
								}
							else										// Altrimenti il comando non è riconosciuto:...
								{										// ...genera eccezione
								throw new Exception(string.Format(MSG.ErroreSintassiSezione, sect, t.Item2, STR_On, STR_Off));
								}
							}
						}
					else if(t.Item1.Length>0)							// Se invece è stata indentificata un'assegnazione di variabile (non di sezione):
						{
						if( (section.Length==0) || ((section.Length>0) && (_sect[section])))	// Se fuori sezione o in una sezione attiva
							{
							TypeVar typ = Assign(t.Item1, t.Item2);		// Esegue l'assegnazione
							}
						}

					linenum++;											// Incrementa il numero di linea

					}
				catch (Exception ex)
					{
					_msg.AppendLine($"Linea [{linenum}]. {ex.Message}");	// Messaggio di errore con numero di linea
					continua = false;
					ok = false;
					}
				} // Fine foreach(...)
			}

		/// <summary>
		/// Identifica il contenuto tra gli indicatori di sezione []
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		string IdentifySection(string line)
			{
			string s = string.Empty;
			string l = line.Trim();

			int i1,i2;
			i1 = l.IndexOf(CHR_SezioneOpenBracket);
			i2 = l.LastIndexOf(CHR_SezioneClosedBracket);
			if( (i1 >= 0) && (i2 >=0) && (i2 > i1) )
				{
				s = l.Substring(i1 + CHR_SezioneOpenBracket.Length, i2 - i1 -1);
				}
			return s;
			}

		/// <summary>
		/// Identifica l'asseganzione di una variabile
		/// Separa le due parti prima e dopo il segno '='
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		Tuple<string, string> IdentifyVariable(string line)
			{
			string v, c;
			v = c = string.Empty;
			int i;
			if( (i = line.IndexOf(CHR_Assign)) != -1)		// Cerca la prima occorrenza del carattere di assegnazione
				{
				v = line.Substring(0,i).Trim();
				c = line.Substring(i+1).Trim();
				}
			return new Tuple<string,string>(v,c);
			}

		/// <summary>
		/// Identifica un comando e lo separa della stringa degli argomenti
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		Tuple<string, string> IdentifyCommand(string line)
			{
			string c, a;
			a = string.Empty;
			int i;
			if( (i = line.IndexOf(CHR_TypeArgSeparator)) != -1)		// Cerca la prima occorrenza del carattere di separazione (spazio)
				{
				c = line.Substring(0,i).Trim();
				a = line.Substring(i+1).Trim();
				}
			else
				{
				c = line;											// Nel caso ci sia solo il omando senza argomenti
				}
			if(!_cmds.ContainsKey(c))								// Cerca il comando. Se non esiste, azzera
				{
				c = a = string.Empty;
				}
			return new Tuple<string,string>(c,a);
			}

		/// <summary>
		/// Separa la stringa in sottostringhe, divise dal separatore di lista,
		/// escludendo i separatori racchiusi tra i delimitatori di stringhe (spazio)
		/// </summary>
		/// <param name="txt"></param>
		/// <returns></returns>
		List<string> ArgList(string txt)
			{
			List<string> lst = new List<string>();

			List<int> indxs = txt.IndexOfOutside(CHR_ListSeparator,CHR_StringDelimiter,CHR_StringDelimiter);

			string tmp;
			int i1, i2;		
			for(int i = 0; i <= indxs.Count; i++)		// Ciclo sul 2° indice fino oltre la lista
				{
				int p1,p2;								// Punti di inizio e fine
				i2 = i;
				i1 = i-1;
				if(i2 == indxs.Count)					// Se la lista è vuota: fa solo il ciclo i = 0 = indx.Count
					p2 = txt.Length;					// p1 = -1, p2 = txt.Length; estrae da p1+1=0 a p2+1-1=p2
				else
					p2 = indxs[i2];
				if(i1 < 0)
					p1 = -1;
				else
					p1 = indxs[i1];

				tmp = txt.Substring(p1+1,p2-p1-1).Trim();	// Estrae tra i due indici e...
				lst.Add(tmp);								// Aggiunge alla lista
				}
			return lst;
			}

		/// <summary>
		/// Estrae tipo e nome della variabile
		/// </summary>
		/// <param name="txt"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		Tuple<TypeVar, string> VarNameAndType(string txt)
			{
			TypeVar t = TypeVar.None;
			string name = string.Empty;
			string type = string.Empty;
			string line = txt.Replace('\t',' ').Trim();				// Sostituisce tab con spazio e toglie gli spazi iniziali e finali
			if(txt.Length > 2)
				{
				int i = line.IndexOf(CHR_TypeArgSeparator);			// Prima occorrenza (tolti spazi iniziali)
				if( (i > 0) && (i < line.Length-1))					// Diverso da -1 e all'interno
					{
					type = line.Substring(0,i).Trim();
					name = line.Substring(i).Trim();
					foreach(TypeVar s in Enum.GetValues(typeof(TypeVar)))	// Cerca il tipo corrispondente
						{
						if(type == s.ToString())
							t = s;
						}
					}
				}
			if(t == TypeVar.None)									// Se non trova corrispondenza: genera eccezione
				{
				if(type.Length > 0)
					throw new Exception(string.Format(MSG.ErroreSintassiTipoNonRiconosciuto, type));
				else
					throw new Exception(string.Format(MSG.ErroreSintassiTipoNonSpecificato));
				}
			return new Tuple<TypeVar, string>(t, name);
			}

		/// <summary>
		/// Assegna 
		/// </summary>
		/// <param name="var"></param>
		/// <param name="val"></param>
		/// <returns></returns>
		TypeVar Assign(string var, string val)
			{ 
			TypeVar typ = TypeVar.None;
			
			List<string> lst = ArgList(val);					// Ottiene i valori degli argomenti (lista)
			Tuple<TypeVar, string> t = VarNameAndType(var);		// Ottiene tipo e nome della variabile	
			
			#if false											// Mostra l'assegnazione (poi eliminare)
			string clst1,clst2,ddd;
			clst1=clst2=ddd=String.Empty;
			if(lst.Count > 1) {clst1 = "{";clst2 = "}";}
			if(val.Length == 0)	ddd=$"ELIMINA[{lst.Count}] ";
			_msg.AppendLine($"{ddd}{clst1}{t.Item1}{clst2} {t.Item2} = {val}");
			#endif

			typ = ExecuteAssign(t.Item1, t.Item2, lst);			// Esegue l'assegnazione

			return typ;
			}
		
		/// <summary>
		/// Converte una stringa nel tipo di dato specificato da un parametro
		/// </summary>
		/// <param name="txt">testo da convertire</param>
		/// <param name="typ">TypeVar</param>
		/// <param name="ok">false se errore</param>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		dynamic ConvertString(string txt, TypeVar typ, out bool ok)
			{
			switch(typ)
				{
				case TypeVar.INT:
					{
					int x;	// TryParse(..., out int result) imposta result a 0 se errore
					ok = int.TryParse(txt, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out x);
					return x;
					}
				case TypeVar.STR:
					{
					ok = true;		// txt, letto dal file di configurazione, contiene anche i doppi apici ""
					return txt.TrimStart('"').TrimEnd('"');
					}
				case TypeVar.BOOL:
					{
					bool x = false;
					if(strTrue.Contains(txt))
						{
						x = true;
						ok = true;
						}
					else if(strFalse.Contains(txt))
						{
						x = false;
						ok = true;
						}
					else
						{
						ok = false;
						}
					return x;
					}
				case TypeVar.FLOAT:
					{
					float x;
					ok = float.TryParse(txt, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out x);
					return x;
					}
				case TypeVar.DOUBLE:
					{
					double x;
					ok = double.TryParse(txt, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out x);
					return x;
					}
				case TypeVar.DATE:
					{
					throw new NotImplementedException("Tipo dato non ancora implementato");
					}
				default:
					{
					ok = false;
					throw new NotImplementedException("Tipo dato non definito");
					}
				}
			}	

		/// <summary>
		/// Crea una lista del tipo di dati specificato
		/// </summary>
		/// <param name="typ"></param>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		dynamic CreateList(TypeVar typ)
			{
			switch(typ)
				{
				case TypeVar.INT:
					{
					return new List<int>();
					}
				case TypeVar.STR:
					{
					return new List<string>();
					}
				case TypeVar.BOOL:
					{
					return new List<bool>();
					}
				case TypeVar.FLOAT:
					{
					return new List<float>();
					}
				case TypeVar.DOUBLE:
					{
					return new List<double>();
					}
				case TypeVar.DATE:
					{
					throw new NotImplementedException("Tipo dato non implementato");
					}
				case TypeVar.COLOR:
					{
					throw new NotImplementedException("Lista di COLOR non ammessa");
					}
				default:
					{
					throw new NotImplementedException("Tipo dato non definito");
					}
				}
			}

		/// <summary>
		/// Esegue l'assegnazione
		/// </summary>
		/// <param name="typ"></param>
		/// <param name="name"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		TypeVar ExecuteAssign(TypeVar typ, string name, List<string> args)
			{
			TypeVar ret = TypeVar.None;							// Nessun tipo di default
			bool ok;
			int n = args.Count;
			if( (n > 0) && (name.Length > 0) )					// Se nome e numero di elementi validi
				{
				if(n == 1)
					{
					dynamic x = ConvertString(args[0], typ, out ok);
					if(ok)
						{
						_dict[name] = x;
						ret = typ;
						}
					else if(args[0].Length == 0)	// Se errore di conversione perché lunghezza nulla:...
						{							// ...allora era richiesta la cancellazione.
						_dict[name] = null;			// Rimuove
						}
					}
				else
					{
					bool oks = true;
					dynamic list = CreateList(typ);				// Crea la lista
					foreach(string arg in args)					// Ripete per tutti gli argomenti
						{
						dynamic x = ConvertString(arg, typ, out ok);
						if(ok)
							{
							list.Add(x);
							}
						else
							{
							oks = false;
							}
						}
					if(oks)
						{
						_dict[name] = list;
						ret = typ;
						}
					}
				}
			return ret;
			}

		/// <summary>
		/// Dynamic Object member access 
		/// </summary>
		/// <param name="binder"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public override bool TryGetMember(GetMemberBinder binder, out dynamic result)
			{
			string key = binder.Name;
			bool ok = true;
			if(_dict.ContainsKey(key))
				{
				result = _dict[key];
				}
			else
				{
				result = null;
				ok = false;
				}
			return ok;
			}

		/// <summary>
		/// Dynamic Object member access 
		/// </summary>
		/// <param name="binder"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public override bool TrySetMember(SetMemberBinder binder, object value)
			{
			_dict[binder.Name] = value;
			return true;
			}

		/// <summary>
		/// Set public members contained in the dictionary
		/// </summary>
		/// <param name="removeFromDictionary">Remove copied entries from dictionary</param>
		/// <returns></returns>
		public bool GetNames(bool removeFromDictionary)
			{
			bool ok = true;

			FieldInfo[] finfos;
			Type myType = this.GetType();
			finfos = myType.GetFields();

			#if false
			_msg.AppendLine($"VARIABILI[{finfos.Length}]");
			foreach(FieldInfo finfo in finfos)
				{
				_msg.AppendLine($"{finfo.Name}= GetValue:{finfo.GetValue(this)} DeclaringType:{finfo.DeclaringType} MemeberType:{finfo.MemberType} fieldType:{finfo.FieldType}");
				}
			#endif

			_msg.AppendLine($"ASSEGNAZIONI");
			foreach(FieldInfo finfo in finfos)
				{		
				if(_dict.ContainsKey(finfo.Name))
					{
					finfo.SetValue(this, _dict[finfo.Name]);
					if(removeFromDictionary)
						{
						_dict[finfo.Name] = null;		// Rimuove dal dizionario
						}
					_msg.AppendLine($"{finfo.Name} = {finfo.GetValue(this)}");
					}
				else
					{
					_msg.AppendLine($"{finfo.Name} non trovato nel file di configurazione");
					ok = false;
					}
				}
			return ok;
			}

		}	// Fine classe CfgReader
	
	
	}	// fine namespace CfgReader
