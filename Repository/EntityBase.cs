using System;

namespace Easy.Common.NetCore.Repository
{
    public class EntityBase
    {
        public int Id { get; set; }

        public string Creater { get; set; }

        public DateTime CreateDate { get; set; } = DateTime.Now;

        public string Editor { get; set; }

        public DateTime? EditDate { get; set; }

        public bool IsDel { get; set; } = false;

        public int Version { get; set; } = 0;
    }
}