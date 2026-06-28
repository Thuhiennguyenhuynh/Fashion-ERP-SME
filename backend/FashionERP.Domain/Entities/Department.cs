using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FashionERP.Domain.Common;

namespace FashionERP.Domain.Entities
{
    /// <summary>
    /// Phòng ban (Bán hàng, Kho, Kế toán...)
    /// </summary>
    public class Department : BaseEntity
    {
        [Required(ErrorMessage = "Tên phòng ban không được để trống")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "Tên phòng ban phải có độ dài từ 2 đến 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string? Description { get; set; }

        /// <summary>Trưởng phòng (nullable)</summary>
        public Guid? ManagerId { get; set; }
        public virtual Employee? Manager { get; set; }

        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}

