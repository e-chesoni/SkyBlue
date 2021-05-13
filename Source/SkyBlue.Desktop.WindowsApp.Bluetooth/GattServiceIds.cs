using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SkyBlue.Desktop.WindowsApp.Bluetooth
{
    class GattServiceIds : IReadOnlyCollection<GattService>
    {
        #region Private Members

        /// <summary>
        /// The backing store for the list of services
        /// </summary>
        private readonly IReadOnlyCollection<GattService> mCollection;

        #endregion

        #region Public Properties

        /// <summary>
        /// Total number of items in the collection
        /// </summary>
        public int Count => mCollection.Count;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public GattServiceIds()
        {
            mCollection = new List<GattService>(new[]
            {
                new GattService("name", "id", 0x1800, "GSS")
            });
        }

        #endregion

        #region IReadOnlyCollection Methods

        /// <summary>
        /// Gets the underlying enumerator of the collection
        /// </summary>
        /// <returns></returns>
        public IEnumerator<GattService> GetEnumerator() => mCollection.GetEnumerator();

        /// <summary>
        /// Gets the underlying enumerator of the collection
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() => mCollection.GetEnumerator();

        #endregion
    }
}
