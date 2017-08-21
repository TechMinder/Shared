using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleInjectorServiceLocatorExample
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IRequestHandlerFactory<T>
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		T CreateNew(string name);
	}


	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class RequestHandlerFactory<T> : Dictionary<string, Type>, IRequestHandlerFactory<T>
	{
		private readonly Container container;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="container"></param>
		public RequestHandlerFactory(Container container)
		{
			this.container = container;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public T CreateNew(string name)
		{
		 return	(T)this.container.GetInstance(this[name]);
		}
	}
}
