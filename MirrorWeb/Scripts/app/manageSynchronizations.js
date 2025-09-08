$(document).ready(function () {

    InitializeSyncGrid();

    var intervalId = window.setInterval(function () {
        UpdateSyncGrid();
    }, 2500);

    
    
});


//Load Snow table-grid
function InitializeSyncGrid() {
    $("#synchronizationGrid").css("display", "block");

    $("#synchronizationGrid").kendoGrid({
        groupable: true,
        sortable: true,
        resizable: false,
        scrollable: true,
        height: 650,
        autoBind: false,
        filterable: {
            operators: {
                string: {
                    eq: "Equal to",
                    neq: "Not equal to",
                    isnull: "Null",
                    isnotnull: "Not null",
                    contains: "Contains",
                    doesnotcontain: "Doesn't contain",
                    startswith: "Starts",
                    endswith: "Ends",
                    doesnotstartwith: "Does not start",
                    doesnotendwith: "Does not end",
                    isempty: "Empty",
                    isnotempty: "Not empty"
                }
            }
        },
        filterMenuInit: initFilterMenuSyncGrid,
        pageable: {
            refresh: true,
            pageSizes: [10, 20, 30, 40, 50],
            previousNext: true,
            width: 160,
            buttonCount: 3,
            messages: {
                display: "{0} - {1} of {2} Synchronizations",
                itemsPerPage: "Synchronizations per Page",
                empty: "No Data",
                allPages: "All"
            }
        },
        dataSource:
        {
            transport: {
                read: {
                    url: "~/../../api/SnowApi/InitializeSyncGrid",
                    dataType: "json",
                    contentType: "application/json",
                    type: 'GET'
                    
                }
            },
            schema: {
                data: function (result) {
                    return result.Synchronizations;
                },
                total: function (result) {
                    return result.SynchronizationCount;
                },
                model: {
                    fields: {
                        Id: { type: "string", editable: false, hidden: true },
                        Running: { type: "boolean", editable: false },
                        Name: { type: "string", editable: false },
                        Synchronization: { type: "string", editable: false },
                        StartTime: { type: "string", editable: false },
                        EndTime: { type: "string", editable: false },
                        Enabled: { type: "boolean", editable: false }
                        
                    }
                }
            },
            change: function (e) {

                if (e.sender._pageSize > 10) {
                    localStorage.setItem("syncGridPageSize", e.sender._pageSize);
                } else {
                    localStorage.setItem("syncGridPageSize", 10);
                }
                e.preventDefault();
            },

            serverPaging: false,
            serverFiltering: false,
            serverSorting: true,
            pageSize: 30
        },
        editable: true,
        columns: [
            {
                field: "Id",
                hidden: true
            }, {
                field: "Enabled",
                title: "Enabled",
                attributes: { style: "text-align:center" },
                template: "#if (Enabled == true) {# <span id='enabled_#=Id#' class='fas fa-check-circle' style='font-size: 16px; color: green;'></span> #} else {# <span id='enabled_#=Id#' class='fas fa-times-circle' style='font-size: 16px; color: red;'></span> #} #",
                width: 25,
                filterable: false,
                sortable: false
            }, {
                field: "Running",
                title: "Running",
                attributes: { style: "text-align:center" },
                template: "#if (Running == true) {# <span id='running_#=Id#' class='fas fa-check-circle' style='font-size: 16px; color: green;'></span> #} else {# <span id='running_#=Id#' class='fas fa-times-circle' style='font-size: 16px; color: red;'></span> #} #",
                width: 25,
                filterable: false,
                sortable: false
            }, {
                field: "Name",
                width: 100,
                template: '<a href=\"#=SyncUrl#\" target="_blank">#=Name#</a>',
                filterable: false,
                sortable: false
            }, {
                title: "Started",
                width: 60,
                template: "<span id='startTime_#=Id#'>#=StartTime#</span>",
                filterable: false,
                sortable: false
            }, {
                title: "Ended",
                width: 60,
                template:"<span id='endTime_#=Id#'>#=EndTime#</span>",
                filterable: false,
                sortable: false
            }, {
                title: "Enable/Disable",
                width: 35,
                attributes: { style: "text-align:center" },
                template: "<input type='button' class='k-button toogleSync_#=kendo.toString(Id)#' id='#=Id#' onclick='ToogleSynchronization($(this));' value='Enable'/>",
            }, {
                title: "Stopp",
                width: 30,
                attributes: { style: "text-align:center" },
                template: "<input type='button' class='k-button toogleProcess_#=kendo.toString(Id)#' id='#=Id#' onclick='StopAllProcessFromSync($(this));' value='Stopp' style='display:none;' />"
            }, {
                title: "Delete",
                width: 33,
                attributes: { style: "text-align:center" },
                template: "<input type='button' name='#=Name#' class='k-button deleteSync_#=kendo.toString(Id)#' id='#=Id#' onclick='DeleteSync($(this));' value='Delete' style='display:none;'/>"
            }
        ]
    }).data("kendoGrid");

    var syncGrid = $("#synchronizationGrid").data("kendoGrid");
    syncGrid.bind("dataBound", syncGridDataBound);
    syncGrid.dataSource.fetch();
}

function syncGridDataBound(e) {

    var filter = this.dataSource.filter();
    this.thead.find(".k-header-column-menu.k-state-active").removeClass("k-state-active");
    if (filter) {
        var filteredMembers = {};
        setFilteredMembers(filter, filteredMembers);
        this.thead.find("th[data-field]").each(function () {
            var cell = $(this);
            var filtered = filteredMembers[cell.data("field")];
            if (filtered) {
                cell.find(".k-header-column-menu").addClass("k-state-active");
            }
        });
    }
}
//Set filtered members
function setFilteredMembers(filter, members) {
    if (filter.filters) {
        for (var i = 0; i < filter.filters.length; i++) {
            setFilteredMembers(filter.filters[i], members);
        }
    }
    else {
        members[filter.field] = true;
    }
}

//init filter
function initFilterMenuSyncGrid(e) {
    var firstDropDown = $('[data-bind="value: filters[0].operator"]').data('kendoDropDownList');
    $('button[type="submit"]').click(function (ev) {

        //reset filterselection
        var fieldType = getFieldType(e.sender.dataSource, e.field);

        if (firstDropDown.value() === 'eq' && fieldType === "date") {
            ev.preventDefault();
            var selectedDate = $('[data-role="datepicker"]').first().data('kendoDatePicker').value();

            if (!selectedDate) {
                $(ev.target).closest('[data-role="popup"]').data('kendoPopup').close();
                return;
            }

            var startOfFilterDateUf = new Date(selectedDate.getFullYear(), selectedDate.getMonth(), selectedDate.getDate());
            var startOfFilterDate = moment(startOfFilterDateUf).format('DD-MM-YYYY');

            var endOfFilterDateUf = new Date(selectedDate.getFullYear(), selectedDate.getMonth(), selectedDate.getDate(), 23, 59, 59);
            var endOfFilterDate = moment(endOfFilterDateUf).format('DD-MM-YYYY');

            var filter = {
                filters: [
                    { field: e.field, operator: "gte", value: startOfFilterDate },
                    { field: e.field, operator: "lte", value: endOfFilterDate }
                ]
            };
            e.sender.dataSource.filter(filter);
            $(ev.target).closest('[data-role="popup"]').data('kendoPopup').close();
            return;
        }
    });
}

function ToogleSynchronization(syncId) {
    if (syncId.length > 0) {

        $.ajax({
            url: "~/../../api/SnowApi/ToogleSynchronization",
            dataType: "json",
            contentType: "application/json",
            type: 'GET',
            data: { synchronizationId: syncId[0].id },
            success: function (res) {
                
            },
            error: function (xhr, status) {
                var err = JSON.parse(xhr.responseText);
                alertify.error(err.Message);
            }
        });
    }
}


function StopAllProcessFromSync(syncId) {
    if (syncId.length > 0) {

        $.ajax({
            url: "~/../../api/SnowApi/StopAllProcessFromSync",
            dataType: "json",
            contentType: "application/json",
            type: 'GET',
            data: { synchronizationId: syncId[0].id },
            success: function (res) {
                if (res !== undefined && res !== "" && res !== null) {

                }
            },
            error: function (xhr, status) {
                var err = JSON.parse(xhr.responseText);
                alertify.error(err.Message);
            }
        });
    }
}

function DeleteSync(syncId) {

    $("#dialog").html("");

    var dialog = $('#kDialog');

    dialog.kendoDialog({
        width: "600px",
        title: "Delete Synchronization/Processes",
        closable: true,
        modal: false,
        content: '<p>Delete Synchronization or/and underlying processes</p>',
        actions: [
            {
                text: "Only Processes",
                action: function (e) {
                    DlgDeleteSync(syncId, false);
                }
            }, {
                text: "Processes & Synchronization",
                action: function (e) {
                    DlgDeleteSync(syncId, true);
                }
            }, {
                text: "Cancel",
                primary: true
            }
        ]
        
    });

    dialog.data("kendoDialog").open();
}

//Delete synchronization and beneath processes
function DlgDeleteSync(syncId, full) {
    if (syncId.length > 0) {

        $.ajax({
            url: "~/../../api/SnowApi/DeleteSynchronization",
            dataType: "json",
            contentType: "application/json",
            type: 'GET',
            data: { synchronizationId: syncId[0].id, delOptionFull: full },
            success: function (res) {
                if (res !== undefined && res !== "" && res !== null) {
                    if (res.SyncDeleted === true) {
                        alertify.success('Synchronization deleted.');
                        InitializeSyncGrid();
                    } else {
                        alertify.error('Synchronization deleting failed. Please see log.');
                    }
                }
            },
            error: function (xhr, status) {
                var err = JSON.parse(xhr.responseText);
                alertify.error(err.Message);
            }
        });
    }
}

//update sync progress in interval
function UpdateSyncGrid() {

    $.ajax({
        url: "~/../../api/SnowApi/InitializeSyncGrid",
        dataType: "json",
        contentType: "application/json",
        type: 'GET',
        success: function (res) {
            if (res !== undefined && res !== "" && res !== null) {

                $.each(res.Synchronizations, function (i, synchronization) {
                    
                    var elementEnabled = $("#enabled_" + synchronization.Id);
                    var oldEnabledClass = elementEnabled.attr("class");
                    
                    if (synchronization.Enabled === true) {
                        elementEnabled.removeClass(oldEnabledClass).addClass('fas fa-check-circle');
                        elementEnabled.css({ 'color': 'green', 'font-size': '16px' });
                    } else {
                        elementEnabled.removeClass(oldEnabledClass).addClass('fas fa-times-circle');
                        elementEnabled.css({ 'color': 'red', 'font-size': '16px' });
                    }

                    var elementRunning = $("#running_" + synchronization.Id);
                    var oldRunningClass = elementRunning.attr("class");

                    if (synchronization.Running === true) {
                        elementRunning.removeClass(oldRunningClass).addClass('fas fa-check-circle');
                        elementRunning.css({ 'color': 'green', 'font-size': '16px' });
                    } else {
                        elementRunning.removeClass(oldRunningClass).addClass('fas fa-times-circle');
                        elementRunning.css({ 'color': 'red', 'font-size': '16px' });
                    }

                    var elementStartTime = $("#startTime_" + synchronization.Id);
                    elementStartTime.text(synchronization.StartTime);

                    var elementEndTime = $("#endTime_" + synchronization.Id);
                    elementEndTime.text(synchronization.EndTime);

                    var elementToogleSync = document.getElementsByClassName('k-button toogleSync_' + synchronization.Id);
                    if (synchronization.Enabled === true) {
                        elementToogleSync[0].value = 'Disable';
                    } else {
                        elementToogleSync[0].value = 'Enable';
                    }
                    
                    var elementStopProcess = document.getElementsByClassName('k-button toogleProcess_' + synchronization.Id);
                    if (synchronization.Running === true) {
                        elementStopProcess[0].style = "display:block";
                    } else {
                        elementStopProcess[0].style = "display:none";
                    }

                    var elementDelSync = document.getElementsByClassName('k-button deleteSync_' + synchronization.Id);
                    if (synchronization.Running === true) {
                        elementDelSync[0].style = "display:none";
                    } else {
                        elementDelSync[0].style = "display:block";
                    }
                });
            }
        },
        error: function (data) {

        }
    });
}