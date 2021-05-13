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
        /// The human readable name for the service
        /// </summary>
        public string Name { get;  }

        /// <summary>
        /// The uniform identifier that is unique to this service
        /// </summary>
        public string UniformTypeIdentifier { get;  }

        /// <summary>
        /// The 16-bit assigned number for this service.
        /// The Bluetooth GATTT Service UUID contains this.
        /// </summary>
        public ushort AssignedNumber { get; }

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
        public GattService(string name, string uniformIdentifier, ushort assignedNumber, string profileSpecification)
        {
            Name = name;
            UniformTypeIdentifier = uniformIdentifier;
            AssignedNumber = assignedNumber;
            ProfileSpecification = profileSpecification;
        }

        #endregion
    }
}
