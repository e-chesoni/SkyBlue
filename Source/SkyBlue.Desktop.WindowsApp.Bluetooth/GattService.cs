using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBlue.Desktop.WindowsApp.Bluetooth
{
    /// <summary>
    /// Details about a specific GATT Service
    /// <seealso cref="https://www.bluetooth.com/specifications/assigned-numbers/"/>
    /// </summary>
    public class GattService
    {
        #region Public Properties
        /// <summary>
        /// GATT value type
        /// </summary>
        public string AllocationType { get;  }

        /// <summary>
        /// The uniform identifier that is unique to this service
        /// </summary>
        public ushort UniformTypeIdentifier { get;  }

        /// <summary>
        /// The human readable name for the service
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The type of specification that this service is.
        /// <seealso cref="https://www.bluetooth.com/specifications/specs/"/>
        /// </summary>
        public string ProfileSpecification { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public GattService(string allocationType, ushort uniformIdentifier, string name, string profileSpecification)
        {
            AllocationType = allocationType;
            UniformTypeIdentifier = uniformIdentifier;
            Name = name;
            ProfileSpecification = profileSpecification;
        }

        #endregion
    }
}
