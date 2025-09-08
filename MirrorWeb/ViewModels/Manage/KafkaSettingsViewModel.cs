using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MirrorRepository.Enums;

namespace MirrorWeb.ViewModels.Manage
{
    public class SyncTargetViewModel : BaseViewModel
    {
        private List<SelectListItem> _targetTypeItem;

        public Guid Id { get; set; }

        public EnumTargetType TargetType { get; set; }

        public string Targetname { get; set; }

        public string Endpoint { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string SelectedSyncTargetType { get; set; }

        public string SelectedSyncTargetName { get; set; }

        public List<SelectListItem> SyncTargets { get; set; }
        
        public List<SelectListItem> SyncTargetTypes
        {
            get => _targetTypeItem = GetTargetTypeFromEnum();
            set => _targetTypeItem = value;
        }

        private List<SelectListItem> GetTargetTypeFromEnum()
        {
            List<SelectListItem> targetTypeItem = Enum.GetValues(typeof(EnumTargetType)).Cast<EnumTargetType>().Select(v => new SelectListItem
            {
                Text = v.ToString(),
                Value = ((int)v).ToString()
            }).ToList();

            return targetTypeItem;
        }
    }
}