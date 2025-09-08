using System;

namespace MirrorWeb.Models
{
    public class CustomDeltaStartModel
    {
        public Guid SynchronizationId { get; set; }

        public string SynchronizationName { get; set; }

        public string CustomDeltaTime { get; set; }
    }
}