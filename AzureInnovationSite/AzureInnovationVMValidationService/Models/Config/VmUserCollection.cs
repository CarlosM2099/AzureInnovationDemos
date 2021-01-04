using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureInnovationVMValidationService.Models.Config
{
    /// <summary>
    /// Represents the "vmPool" element in app.config.
    /// </summary>
    /// <seealso cref="ConfigurationElementCollection" />
    [SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface",
        Justification="Nothing would use it; just call the .Items property.")]
    [ConfigurationCollection(typeof(VmUser), AddItemName = "add", ClearItemsName = "clear",
        RemoveItemName = "remove")]
    public class VmUserCollection : ConfigurationElementCollection
    {
        public VmUser this[int index]
        {
            get
            {
                return base.BaseGet(index) as VmUser;
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }

        /// <summary>
        /// When overridden in a derived class, creates a new
        /// <see cref="T:System.Configuration.ConfigurationElement" />.
        /// </summary>
        /// <returns>
        /// A newly created <see cref="T:System.Configuration.ConfigurationElement" />.
        /// </returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new VmUser();
        }

        /// <summary>
        /// Gets the element key for a specified configuration element when overridden in a derived
        /// class.
        /// </summary>
        /// <param name="element">
        /// The <see cref="T:System.Configuration.ConfigurationElement" /> to return the key for.
        /// </param>
        /// <returns>
        /// An <see cref="T:System.Object" /> that acts as the key for the specified
        /// <see cref="T:System.Configuration.ConfigurationElement" />.
        /// </returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as VmUser)?.RemoteDesktopUserName;
        }

        public IEnumerable<VmUser> Items
        {
            get
            {
                foreach (var item in (IEnumerable)this)
                {
                    yield return item as VmUser;
                }
            }
        }
    }
}
