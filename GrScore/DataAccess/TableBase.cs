/*
 *  Sqlite ORM - PUBLIC DOMAIN LICENSE
 *  Copyright (C)  2010-2012. Ian Quigley
 *  
 *  This source code is provided 'As is'. You bear the risk of using it.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using SqliteORM;
using System.ComponentModel;

namespace GrScore.DataAccess.DataBaseAccess
{
    [Serializable]
	public abstract class TableBase<T> : INotifyPropertyChanged
	{
		public virtual void Save( )
		{
			using (TableAdapter<T> adapter = TableAdapter<T>.Open())
				adapter.CreateUpdate( this );
		}
		
		public static void Do( Action<TableAdapter<T>> action ) 
		{            
            using (TableAdapter<T> adapter = TableAdapter<T>.Open())
                action( adapter );            
		}

        public static void Delete( params object[] args )
        {
            using (TableAdapter<T> adapter = TableAdapter<T>.Open())
                adapter.Delete( args );
        }

        public static T Read( params object[] args )
        {
            using (TableAdapter<T> adapter = TableAdapter<T>.Open())
                return adapter.Read( args );
        }

        public static TReturn Get<TReturn>( Func<TableAdapter<T>, TReturn> action ) where TReturn : class
		{
            using (TableAdapter<T> adapter = TableAdapter<T>.Open())
                return action( adapter );
		}

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion // INotifyPropertyChanged Members
	}
}
