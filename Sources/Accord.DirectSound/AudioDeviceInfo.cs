﻿// Accord (Experimental) Audio Library
// The Accord.NET Framework
// http://accord-net.origo.ethz.ch
//
// Copyright © César Souza, 2009-2011
// cesarsouza at gmail.com
//
//    This library is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Lesser General Public
//    License as published by the Free Software Foundation; either
//    version 2.1 of the License, or (at your option) any later version.
//
//    This library is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Lesser General Public
//    License along with this library; if not, write to the Free Software
//    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

namespace Accord.DirectSound
{
    using System;
    using SlimDX.DirectSound;

    /// <summary>
    ///   Audio device information.
    /// </summary>
    /// 
    public class AudioDeviceInfo
    {

        private DeviceInformation info;

        /// <summary>
        ///   Gets the description of the device.
        /// </summary>
        /// 
        /// <value>The description of the device.</value>
        /// 
        public string Description
        {
            get { return info.Description; }
        }

        /// <summary>
        ///   Gets the unique id of the device.
        /// </summary>
        /// 
        /// <value>The <see cref="Guid">Global Unique Identifier</see> of the device.</value>
        /// 
        public Guid Guid
        {
            get { return info.DriverGuid; }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> representing the audio device.
        /// </summary>
        /// 
        /// <returns>
        /// A <see cref="System.String"/> that represents the audio device.
        /// </returns>
        /// 
        public override string ToString()
        {
            return Description;
        }


        internal AudioDeviceInfo(DeviceInformation info)
        {
            this.info = info;            
        }

    }
}
