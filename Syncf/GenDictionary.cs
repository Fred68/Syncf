#if DYN_DICTIONARY
#undef DYN_DICTIONARY
#endif
#define DYN_DICTIONARY			// Use ctor: dynamic GenDictionary(); instead of GenDictionary GenDictionary();

using System;
using System.Collections.Generic;
using System.Text;

#if DYN_DICTIONARY
using System.Dynamic;
#endif

namespace Fred68.GenDictionary
	{

	public class GenDictionary 
		#if DYN_DICTIONARY
			: DynamicObject
		#endif
		{
		/// <summary>
		/// Dizionario di blocchi di dati Dat
		/// </summary>
		private Dictionary<string, Dat> _dict;
		
		/// <summary>
		/// Ctor
		/// </summary>
		public GenDictionary()
			{
			_dict = new Dictionary<string, Dat>();
			}

		/// <summary>
		/// Accesso
		/// dict[key]=value: Aggiunge, aggiorna o elimina se value è null
		/// dict[key]: accede in lettura.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		/// <exception cref="KeyNotFoundException"></exception>
		public dynamic this[string key]
			{		
			get			// Usa la funzione dynamic Get() definita per la classe Dat
				{
				if(_dict.ContainsKey(key))
					return (dynamic)_dict[key].Get();
				else
					throw new KeyNotFoundException();
				}
			set			// Se null: rimuove
				{
				var x = value;
				if(x == null)
					{
					_dict.Remove(key);
					}
				else
					{
					_dict[key] = new Dat(x);
					}
				}
			}

		#if DYN_DICTIONARY
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
				result = this[key];
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
			this[binder.Name] = value;
			return true;
			}
		#endif

		/// <summary>
		/// Dumpe dictionary content to string
		/// </summary>
		/// <returns></returns>
		public string Dump()
			{
			StringBuilder sb = new StringBuilder();
			foreach(string key in _dict.Keys)
				{
				if(_dict[key].IsList)
					{
					StringBuilder l = new StringBuilder();
					l.Append($"[{key}]=");
					dynamic lst = _dict[key].Get();
					l.Append($"[{lst.Count}]"+"{");
					for(int i=0; i < lst.Count; i++)
						{
						dynamic x = lst[i];
						l.Append(x);
						l.Append( (i!=lst.Count-1) ? "; " : "}");
						}
					sb.AppendLine(l.ToString());
					}
				else
					{
					sb.AppendLine($"[{key}]={_dict[key].Get().ToString()}");
					}
				}
			
			return sb.ToString();
			}

		#region IDictionary (solo alcune funzioni)
        public bool ContainsKey(string key)	{return _dict.ContainsKey(key);}
		public bool Contains(object item)	{throw new NotImplementedException("Funzione Contains non implementata");}
		public int Count {get {return _dict.Count;}}
		public ICollection<string> KeyCollection {get {return _dict.Keys;}}
		public ICollection<object> Values {get {throw new NotImplementedException("Proprietà Values non implementata");}}
		public void Clear() {_dict.Clear();}
		public IEnumerable<string> Keys()
			{
            foreach(string s in _dict.Keys)
				yield return s;
			}
		public IEnumerator<string> GetEnumerator()
			{
			foreach(string s in _dict.Keys)
				yield return s;
			}
		#endregion

		}

	}
