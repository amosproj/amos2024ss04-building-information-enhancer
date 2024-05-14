using System.ComponentModel.DataAnnotations;

namespace BIE.Core.DataObjects
{
    public class Infrastructure 
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public long? InfrastructureID { get; set; }

        /// <summary>
        /// Short name 
        /// </summary>
        [Required]
        public string ShortName { get; set; }

        /// <summary>
        /// Name 
        /// </summary>
        [Required]
        public string Name { get; set; }


        /// <summary>
        /// get the current instance BatchID 
        /// </summary>
        //public long Key => this.BatchID.Value;

        //public void Copy(IEntity entity)
        //{
        //    Helper.Copy(typeof(Batch), this, entity);
        //}
    }
}
