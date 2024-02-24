using System;
using System.Collections.Generic;
//	using System.Linq;
//	using System.Text;
//	using System.Threading.Tasks;
//	using System.Reflection;

namespace Fred68.GenDictionary
	{

	/// <summary>
	/// Tipi di dati trattati
	/// </summary>
	public enum TypeVar
		{
		INT,
		STR,
		BOOL,
		FLOAT,
		DOUBLE,
		DATE,
		COLOR,				// ARGB
		None				// Ultimo 
		}

	/// <summary>
	/// Classe Dat: oggetto generico con associato il tipo di dato.
	/// La classe non è generica, per poter esser contenuta in un unico raccoglitore
	/// </summary>
	public class Dat
		{

		static TypeVar[] _tc;

		TypeVar _t;				// Tipo di dato
		object _obj;			// Oggetto
		bool _list;				// true se l'oggetto è una lista

		static Dat()
			{
			_tc = new TypeVar[((int)TypeVar.None)-1];
			}

		public static Type GetEqType(dynamic x)
			{
			return x.GetType();
			
			}
		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="t"></param>
		/// <param name="_d"></param>
		public Dat(int _d)
			{
			_t = TypeVar.INT;
			_obj = _d;
			_list = false;
			}
		public Dat(List<int> _d)
			{
			_t = TypeVar.INT;
			_obj = _d;
			_list = true;
			}
		public Dat(string _d)
			{
			_t = TypeVar.STR;
			_obj = _d;
			_list = false;
			}
		public Dat(List<string> _d)
			{
			_t = TypeVar.STR;
			_obj = _d;
			_list = true;
			}
		public Dat(bool _d)
			{
			_t = TypeVar.BOOL;
			_obj = _d;
			_list = false;
			}
		public Dat(List<bool> _d)
			{
			_t = TypeVar.BOOL;
			_obj = _d;
			_list = true;
			}		
		public Dat(float _d)
			{
			_t = TypeVar.FLOAT;
			_obj = _d;
			_list = false;
			}
		public Dat(List<float> _d)
			{
			_t = TypeVar.FLOAT;
			_obj = _d;
			_list = true;
			}
		public Dat(double _d)
			{
			_t = TypeVar.DOUBLE;
			_obj = _d;
			_list = false;
			}
		public Dat(List<double> _d)
			{
			_t = TypeVar.DOUBLE;
			_obj = _d;
			_list = true;
			}
		public Dat(DateTime _d)
			{
			_t = TypeVar.DATE;
			_obj = _d;
			_list = false;
			}
		public Dat(List<DateTime> _d)
			{
			_t = TypeVar.DATE;
			_obj = _d;
			_list = true;
			}
		
		/// <summary>
		/// Dat contains a list
		/// </summary>
		public bool IsList
			{
			get { return _list; }
			}

		/// <summary>
		/// Restituisce l'oggetto, riconvertito al tipo di dato originario.
		/// La dichiarazione è dynamic, per avere un'unica funzione Get
		/// </summary>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public dynamic Get()
			{
			switch(_t)
				{
				case TypeVar.INT:
					{
					if(_list)
						return (List<int>)_obj;
					else
						return (int)_obj;
					}
					//break;
				case TypeVar.STR:
					{
					if(_list)
						return (List<string>)_obj;
					else
						return (string)_obj;
					}
					//break;
				case TypeVar.BOOL:
					{
					if(_list)
						return (List<bool>)_obj;
					else
						return (bool)_obj;
					}
					//break;
				case TypeVar.FLOAT:
					{
					if(_list)
						return (List<float>)_obj;
					else
						return (float)_obj;
					}
					//break;
				case TypeVar.DOUBLE:
					{
					if(_list)
						return (List<double>)_obj;
					else
						return (double)_obj;
					}
					//break;
				case TypeVar.DATE:
					{
					if(_list)
						return (List<DateTime>)_obj;
					else
						return (DateTime)_obj;
					}
				
				default:
					throw new NotImplementedException("Tipo dato non definito");
				}
			}
		}
	}
