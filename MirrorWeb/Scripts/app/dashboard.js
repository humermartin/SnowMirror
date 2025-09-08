var intervalId;

$(document).ready(function () {
    
    GetDashboardData();

    RestartInterval();
});

//restart interval
function RestartInterval() {
    clearInterval(intervalId);

    intervalId = window.setInterval(function () {
        ReloadDashboard();
    }, 5000);
}

//reload dashboard
function ReloadDashboard() {

    var url = "~/../../api/SnowApi/GetDashboardSyncModel";
    if (vPath != null || vPath != undefined || vPath != "") {
        url = "~/" + vPath + "/../../api/SnowApi/GetDashboardSyncModel";
    }

    $.ajax({
        url: url,
        dataType: "json",
        contentType: "application/json",
        type: 'GET',
        success: function (dashboardModel) {
            if (dashboardModel !== undefined && dashboardModel !== "" && dashboardModel !== null) {

                InitRunningSyncGrid(dashboardModel);
                //reload running syncs
                /*
                $.each(dashboardModel.RunningSyncViewModel, function (i, item) {

                    if (item.TableName != null) {
                        $('#run_tableName_' + item.Id).text(item.TableName);
                    } else {
                        $('#run_tableName_' + item.Id).text("");
                    }

                    if (item.StartTime != null) {
                        $('#run_startTime_' + item.Id).text(item.StartTime);
                    } else {
                        $('#run_startDate_' + item.Id).text("");
                    }

                    if (item.RecordsFound != null) {
                        $('#run_recordsFound_' + item.Id).text(item.RecordsFound);
                    } else {
                        $('#run_recordsFound_' + item.Id).text("");
                    }

                    if (item.RecordsSynchronized != null) {
                        $('#run_recordsSynchronized_' + item.Id).text(item.RecordsSynchronized);
                    } else {
                        $('#run_recordsSynchronized_' + item.Id).text("");
                    }

                });
                */

                //reload full syncs
                $.each(dashboardModel.SyncFullViewModels, function (i, item) {
                    
                    if (item.Instance != null) {
                        $('#full_instance_' + item.Id).text(item.Instance);
                    } else {
                        $('#full_instance_' + item.Id).text("");
                    }

                    if (item.PlannedWeekDay != null) {
                        $('#full_plannedWeekDay_' + item.Id).text(item.PlannedWeekDay);
                    } else {
                        $('#full_plannedWeekDay_' + item.Id).text("");
                    }

                    if (item.PlannedStart != null) {
                        $('#full_plannedStart_' + item.Id).text(item.PlannedStart);
                    } else {
                        $('#full_plannedStart_' + item.Id).text("");
                    }

                    if (item.NextStart != null) {
                        $('#full_nextStart_' + item.Id).text(item.NextStart);
                    } else {
                        $('#full_nextStart_' + item.Id).text("");
                    }

                    if (item.Duration != null) {
                        $('#full_duration_' + item.Id).text(item.Duration);
                    } else {
                        $('#full_duration_' + item.Id).text("");
                    }

                    if (item.StartTime != null) {
                        $('#full_startTime_' + item.Id).text(item.StartTime);
                    } else {
                        $('#full_startTime_' + item.Id).text("");
                    }

                    if (item.EndTime != null) {
                        $('#full_endTime_' + item.Id).text(item.EndTime);
                    } else {
                        $('#full_endTime_' + item.Id).text("");
                    }
                });

                //reload delta syncs
                $.each(dashboardModel.SyncDeltaViewModels, function (i, item) {

                    if (item.Enabled != null) {
                        $('#delta_enabled_' + item.Id).text(item.Enabled);
                    } else {
                        $('#delta_enabled_' + item.Id).text("");
                    }

                    if (item.Instance != null) {
                        $('#delta_instance_' + item.Id).text(item.Instance);
                    } else {
                        $('#delta_instance_' + item.Id).text("");
                    }

                    if (item.Period != null) {
                        $('#delta_period_' + item.Id).text(item.Period);
                    } else {
                        $('#delta_period_' + item.Id).text("");
                    }

                    if (item.Duration != null) {
                        $('#delta_duration_' + item.Id).text(item.Duration);
                    } else {
                        $('#delta_duration_' + item.Id).text("");
                    }

                    if (item.StartTime != null) {
                        $('#delta_startTime_' + item.Id).text(item.StartTime);
                    } else {
                        $('#delta_startTime_' + item.Id).text("");
                    }

                    if (item.EndTime != null) {
                        $('#delta_endTime_' + item.Id).text(item.EndTime);
                    } else {
                        $('#delta_endTime_' + item.Id).text("");
                    }
                });

                //reload Kafka delta syncs
                $.each(dashboardModel.SyncKafkaDeltaViewModels, function (i, item) {

                    if (item.Enabled != null) {
                        $('#kafka_delta_enabled_' + item.Id).text(item.Enabled);
                    } else {
                        $('#kafka_delta_enabled_' + item.Id).text("");
                    }

                    if (item.Instance != null) {
                        $('#kafka_delta_instance_' + item.Id).text(item.Instance);
                    } else {
                        $('#kafka_delta_instance_' + item.Id).text("");
                    }

                    if (item.Period != null) {
                        $('#kafka_delta_period_' + item.Id).text(item.Period);
                    } else {
                        $('#kafka_delta_period_' + item.Id).text("");
                    }

                    if (item.Duration != null) {
                        $('#kafka_delta_duration_' + item.Id).text(item.Duration);
                    } else {
                        $('#kafka_delta_duration_' + item.Id).text("");
                    }

                    if (item.StartTime != null) {
                        $('#kafka_delta_startTime_' + item.Id).text(item.StartTime);
                    } else {
                        $('#kafka_delta_startTime_' + item.Id).text("");
                    }

                    if (item.EndTime != null) {
                        $('#kafka_delta_endTime_' + item.Id).text(item.EndTime);
                    } else {
                        $('#kafka_delta_endTime_' + item.Id).text("");
                    }
                });

            }
        },
        error: function (xhr, status) {
            var err = JSON.parse(xhr.responseText);
            alertify.error(err.Message);
        }
    });
}

//get dashboard model
function GetDashboardData() {

    var url = "~/../../api/SnowApi/GetDashboardSyncModel";
    if (vPath != null || vPath != undefined || vPath != "") {
        url = "~/" + vPath + "/../../api/SnowApi/GetDashboardSyncModel";
    }

    $.ajax({
        url: url,
        dataType: "json",
        contentType: "application/json",
        type: 'GET',
        success: function (dashboardModel) {
            if (dashboardModel !== undefined && dashboardModel !== "" && dashboardModel !== null) {
                InitRunningSyncGrid(dashboardModel);
                InitPlannedFullSyncGrid(dashboardModel);
                InitPlannedDeltaSyncGrid(dashboardModel);
                InitKafkaDeltaSyncGrid(dashboardModel);
            }
        },
        error: function (xhr, status) {
            var err = JSON.parse(xhr.responseText);
            alertify.error(err.Message);
        }
    });
}

function InitPlannedFullSyncGrid(plannedFullSyncDataSource) {
    $("#plannedFullSyncGrid").css("display", "block");

    $("#plannedFullSyncGrid").kendoGrid({
        groupable: false,
        sortable: true,
        resizable: false,
        scrollable: true,
        height: 450,
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
        filterMenuInit: initFilterDashboardGrid,
        pageable: false,
        dataSource:
        {
            transport: {
                read: function (options) {
                    options.success(plannedFullSyncDataSource);
                }
            },
            schema: {
                data: function (result) {
                    return result.SyncFullViewModels;
                },
                total: function (result) {
                    return result.SyncFullViewModelsTotalCount;
                },
                model: {
                    fields: {
                        Id: { type: "string", editable: false, hidden: true },
                        SyncName: { type: "string", editable: false },
                        Instance: { type: "string", editable: false },
                        PlannedWeekDay: { type: "string", editable: false },
                        PlannedStart: { type: "string", editable: false },
                        NextStart: { type: "string", editable: false },
                        Duration: { type: "string", editable: false },
                        StartTime: { type: "string", editable: false },
                        EndTime: { type: "string", editable: false }
                    }
                }
            },
            change: function (e) {

                if (e.sender._pageSize > 10) {
                    localStorage.setItem("plannedFullSyncGridPageSize", e.sender._pageSize);
                } else {
                    localStorage.setItem("plannedFullSyncGridPageSize", 10);
                }
                e.preventDefault();
            },
            sort: { field: "Instance", dir: "desc" },
            serverPaging: false,
            serverFiltering: false,
            serverSorting: false,
            pageSize: plannedFullSyncDataSource.SyncFullViewModelsTotalCount
        },
        editable: true,
        columns: [
            {
                field: "Id",
                hidden: true
            }, {
                field: "SyncName",
                title: "SyncName",
                template: "#if (SyncName != null && Enabled == false) {# <div id='full_syncName_#=Id#'><span style='color:red'>#=SyncName#</span></div> #} else if" +
                    " (SyncName != null && Enabled == true) {# <div id='full_syncName_#=Id#'><span style='color:green'>#=SyncName#</span></div> #} else " +
                    " { # <div id='full_syncName_#=Id#'><span></span></div> # } #",
                width: 60,
                filterable: true,
                sortable: true
           }, {
                field: "Instance",
                title: "Instance",
                template: "#if (Instance != null) {# <div id='full_instance_#=Id#'><span>#=Instance#</span></div> #} else" +
                    " { # <div id='full_instance_#=Id#'><span></span></div> # } #",
                width: 45,
                filterable: true,
                sortable: true
            }, {
                field: "PlannedWeekDay",
                title: "Day",
                template: "#if (PlannedWeekDay != null) {# <div id='full_plannedWeekDay_#=Id#'><span>#=PlannedWeekDay#</span></div> #} else" +
                    " { # <div id='full_plannedWeekDay_#=Id#'><span></span></div> # } #",
                width: 30,
                filterable: true,
                sortable: true
            }, {
                field: "PlannedStart",
                title: "Time",
                template: "#if (PlannedStart != null) {# <div id='full_plannedStart_#=Id#'><span>#=PlannedStart#</span></div> #} else" +
                    " { # <div id='full_plannedStart_#=Id#'><span></span></div> # } #",
                width: 20,
                filterable: false,
                sortable: true
            }, {
                field: "NextStart",
                title: "Next Start",
                template: "#if (NextStart != null) {# <div id='full_nextStart_#=Id#'><span>#=NextStart#</span></div> #} else" +
                    " { # <div id='full_nextStart_#=Id#'><span></span></div> # } #",
                width: 60,
                filterable: true,
                sortable: true
            }, {
                field: "Duration",
                title: "Duration",
                template: "#if (Duration != null) {# <div id='full_duration_#=Id#'><span>#=Duration#</span></div> #} else" +
                    " { # <div id='full_duration_#=Id#'><span></span></div> # } #",
                width: 30,
                filterable: false,
                sortable: true
            }, {
                field: "StartTime",
                title: "StartTime",
                template: "#if (StartTime != null) {# <div id='full_startTime_#=Id#'><span>#=StartTime#</span></div> #} else" +
                    " { # <div id='full_startTime_#=Id#'><span></span></div> # } #",
                width: 60,
                filterable: false,
                sortable: true
            }, {
                field: "EndTime",
                title: "EndTime",
                template: "#if (EndTime != null) {# <div id='full_endTime_#=Id#'><span>#=EndTime#</span></div> #} else" +
                    " { # <div id='full_endTime_#=Id#'><span></span></div> # } #",
                width: 60,
                filterable: false,
                sortable: true
            }
        ]
    }).data("kendoGrid");

    var plannedFullSyncGrid = $("#plannedFullSyncGrid").data("kendoGrid");
    plannedFullSyncGrid.bind("dataBound", plannedFullSyncGridDataBound);
    plannedFullSyncGrid.dataSource.fetch();
}

function plannedFullSyncGridDataBound(e) {

    var filter = this.dataSource.filter();
    this.thead.find(".k-header-column-menu.k-state-active").removeClass("k-state-active");
    if (filter) {
        var filteredMembers = {};
        setFilteredMembersDashboard(filter, filteredMembers);
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
function setFilteredMembersDashboard(filter, members) {
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
function initFilterDashboardGrid(e) {
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

//Delta Grid
function InitPlannedDeltaSyncGrid(plannedDeltaSyncDataSource) {
    $("#plannedDeltaSyncGrid").css("display", "block");

    $("#plannedDeltaSyncGrid").kendoGrid({
        groupable: false,
        sortable: true,
        resizable: false,
        scrollable: true,
        height: 450,
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
        filterMenuInit: initFilterDashboardGrid,
        pageable: false,
        dataSource:
        {
            transport: {
                read: function (options) {
                    options.success(plannedDeltaSyncDataSource);
                }
            },
            schema: {
                data: function (result) {
                    return result.SyncDeltaViewModels;
                },
                total: function (result) {
                    return result.SyncDeltaViewModelsTotalCount;
                },
                model: {
                    fields: {
                        Id: { type: "string", editable: false, hidden: true },
                        SyncName: { type: "string", editable: false },
                        Enabled: { type: "boolean", editable: false },
                        Instance: { type: "string", editable: false },
                        Period: { type: "number", editable: false },
                        Duration: { type: "string", editable: false },
                        StartTime: { type: "string", editable: false },
                        EndTime: { type: "string", editable: false }
                    }
                }
            },
            change: function (e) {

                if (e.sender._pageSize > 10) {
                    localStorage.setItem("plannedDeltaSyncGridPageSize", e.sender._pageSize);
                } else {
                    localStorage.setItem("plannedDeltaSyncGridPageSize", 10);
                }
                e.preventDefault();
            },
            sort: { field: "Instance", dir: "desc" },
            serverPaging: false,
            serverFiltering: false,
            serverSorting: false,
            pageSize: plannedDeltaSyncDataSource.SyncDeltaViewModelsTotalCount
        },
        editable: true,
        columns: [
            {
                field: "Id",
                hidden: true
            }, {
                field: "SyncName",
                title: "SyncName",
                template: "#if (SyncName != null) {# <div id='delta_syncName_#=Id#'><span>#=SyncName#</span></div> #} else" +
                    " { # <div id='delta_syncName_#=Id#'><span></span></div> # } #",
                width: 80,
                filterable: true,
                sortable: true
            }, {
                field: "Enabled",
                title: "Enabled",
                template: "#if (Enabled != null) {# <div id='delta_enabled_#=Id#'><span>#=Enabled#</span></div> #} else" +
                    " { # <div id='delta_enabled_#=Id#'><span></span></div> # } #",
                width: 40,
                filterable: true,
                sortable: true
            }, {
                field: "Instance",
                title: "Instance",
                template: "#if (Instance != null) {# <div id='delta_instance_#=Id#'><span>#=Instance#</span></div> #} else" +
                    " { # <div id='delta_instance_#=Id#'><span></span></div> # } #",
                width: 60,
                filterable: true,
                sortable: true
            }, {
                field: "Period",
                title: "Period",
                template: "#if (Period != null) {# <div id='delta_period_#=Id#'><span>#=Period#</span></div> #} else" +
                    " { # <div id='delta_period_#=Id#'><span></span></div> # } #",
                width: 60,
                filterable: true,
                sortable: true
            }, {
                field: "Duration",
                title: "Duration",
                template: "#if (Duration != null) {# <div id='delta_duration_#=Id#'><span>#=Duration#</span></div> #} else" +
                    " { # <div id='delta_duration_#=Id#'><span></span></div> # } #",
                width: 60,
                filterable: false,
                sortable: true
            }, {
                field: "StartTime",
                title: "StartTime",
                template: "#if (StartTime != null) {# <div id='delta_startTime_#=Id#'><span>#=StartTime#</span></div> #} else" +
                    " { # <div id='delta_startTime_#=Id#'><span></span></div> # } #",
                width: 60,
                filterable: false,
                sortable: true
            }, {
                field: "EndTime",
                title: "EndTime",
                template: "#if (EndTime != null) {# <div id='delta_endTime_#=Id#'><span>#=EndTime#</span></div> #} else" +
                    " { # <div id='delta_endTime_#=Id#'><span></span></div> # } #",
                width: 60,
                filterable: false,
                sortable: true
            }
        ]
    }).data("kendoGrid");

    var plannedDeltaSyncGrid = $("#plannedDeltaSyncGrid").data("kendoGrid");
    plannedDeltaSyncGrid.bind("dataBound", plannedDeltaSyncGridDataBound);
    plannedDeltaSyncGrid.dataSource.fetch();
}

function plannedDeltaSyncGridDataBound(e) {

    var filter = this.dataSource.filter();
    this.thead.find(".k-header-column-menu.k-state-active").removeClass("k-state-active");
    if (filter) {
        var filteredMembers = {};
        setFilteredMembersDashboard(filter, filteredMembers);
        this.thead.find("th[data-field]").each(function () {
            var cell = $(this);
            var filtered = filteredMembers[cell.data("field")];
            if (filtered) {
                cell.find(".k-header-column-menu").addClass("k-state-active");
            }
        });
    }
}

//Kafka Delta Grid
function InitKafkaDeltaSyncGrid(kafkaDeltaSyncDataSource) {
    $("#kafkaDeltaSyncGrid").css("display", "block");

    $("#kafkaDeltaSyncGrid").kendoGrid({
        groupable: false,
        sortable: true,
        resizable: false,
        scrollable: true,
        height: 450,
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
        filterMenuInit: initFilterDashboardGrid,
        pageable: false,
        dataSource:
        {
            transport: {
                read: function (options) {
                    options.success(kafkaDeltaSyncDataSource);
                }
            },
            schema: {
                data: function (result) {
                    return result.SyncKafkaDeltaViewModels;
                },
                total: function (result) {
                    return result.SyncKafkaDeltaViewModelsTotalCount;
                },
                model: {
                    fields: {
                        Id: { type: "string", editable: false, hidden: true },
                        SyncName: { type: "string", editable: false },
                        Enabled: { type: "boolean", editable: false },
                        Instance: { type: "string", editable: false },
                        Period: { type: "number", editable: false },
                        Duration: { type: "string", editable: false },
                        StartTime: { type: "string", editable: false },
                        EndTime: { type: "string", editable: false }
                    }
                }
            },
            change: function (e) {

                if (e.sender._pageSize > 10) {
                    localStorage.setItem("plannedDeltaSyncGridPageSize", e.sender._pageSize);
                } else {
                    localStorage.setItem("plannedDeltaSyncGridPageSize", 10);
                }
                e.preventDefault();
            },
            sort: { field: "Instance", dir: "desc" },
            serverPaging: false,
            serverFiltering: false,
            serverSorting: false,
            pageSize: kafkaDeltaSyncDataSource.SyncKafkaDeltaViewModelsTotalCount
        },
        editable: true,
        columns: [
            {
                field: "Id",
                hidden: true
            }, {
                field: "SyncName",
                title: "SyncName",
                template: "#if (SyncName != null) {# <div id='kafka_delta_syncName_#=Id#'><span>#=SyncName#</span></div> #} else" +
                    " { # <div id='kafka_delta_syncName_#=Id#'><span></span></div> # } #",
                width: 80,
                filterable: true,
                sortable: true
            }, {
                field: "Enabled",
                title: "Enabled",
                template: "#if (Enabled != null) {# <div id='kafka_delta_enabled_#=Id#'><span>#=Enabled#</span></div> #} else" +
                    " { # <div id='kafka_delta_enabled_#=Id#'><span></span></div> # } #",
                width: 40,
                filterable: true,
                sortable: true
            }, {
                field: "Instance",
                title: "Instance",
                template: "#if (Instance != null) {# <div id='kafka_delta_instance_#=Id#'><span>#=Instance#</span></div> #} else" +
                    " { # <div id='kafka_delta_instance_#=Id#'><span></span></div> # } #",
                width: 60,
                filterable: true,
                sortable: true
            }, {
                field: "Period",
                title: "Period",
                template: "#if (Period != null) {# <div id='kafka_delta_period_#=Id#'><span>#=Period#</span></div> #} else" +
                    " { # <div id='kafka_delta_period_#=Id#'><span></span></div> # } #",
                width: 60,
                filterable: true,
                sortable: true
            }, {
                field: "Duration",
                title: "Duration",
                template: "#if (Duration != null) {# <div id='kafka_delta_duration_#=Id#'><span>#=Duration#</span></div> #} else" +
                    " { # <div id='kafka_delta_duration_#=Id#'><span></span></div> # } #",
                width: 60,
                filterable: false,
                sortable: true
            }, {
                field: "StartTime",
                title: "StartTime",
                template: "#if (StartTime != null) {# <div id='kafka_delta_startTime_#=Id#'><span>#=StartTime#</span></div> #} else" +
                    " { # <div id='kafka_delta_startTime_#=Id#'><span></span></div> # } #",
                width: 60,
                filterable: false,
                sortable: true
            }, {
                field: "EndTime",
                title: "EndTime",
                template: "#if (EndTime != null) {# <div id='kafka_delta_endTime_#=Id#'><span>#=EndTime#</span></div> #} else" +
                    " { # <div id='kafka_delta_endTime_#=Id#'><span></span></div> # } #",
                width: 60,
                filterable: false,
                sortable: true
            }
        ]
    }).data("kendoGrid");

    var kafkaDeltaSyncGrid = $("#kafkaDeltaSyncGrid").data("kendoGrid");
    kafkaDeltaSyncGrid.bind("dataBound", kafkaDeltaSyncGridDataBound);
    kafkaDeltaSyncGrid.dataSource.fetch();
}

function kafkaDeltaSyncGridDataBound(e) {

    var filter = this.dataSource.filter();
    this.thead.find(".k-header-column-menu.k-state-active").removeClass("k-state-active");
    if (filter) {
        var filteredMembers = {};
        setFilteredMembersDashboard(filter, filteredMembers);
        this.thead.find("th[data-field]").each(function () {
            var cell = $(this);
            var filtered = filteredMembers[cell.data("field")];
            if (filtered) {
                cell.find(".k-header-column-menu").addClass("k-state-active");
            }
        });
    }
}

//running synchronizations
function InitRunningSyncGrid(runningSyncDataSource) {
    if (runningSyncDataSource.RunningSyncViewModelTotalCount > 0) {
        $("#runningSyncGrid").css("display", "block");
    } else {
        $("#runSyncCount").text("");
        $("#runningSyncGrid").css("display", "none");
    }
    
    $("#runningSyncGrid").kendoGrid({
        groupable: false,
        sortable: true,
        resizable: false,
        scrollable: true,
        height: 340,
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
        filterMenuInit: initFilterDashboardGrid,
        pageable: false,
        dataSource:
        {
            transport: {
                read: function (options) {
                    options.success(runningSyncDataSource);
                }
            },
            schema: {
                data: function (result) {
                    return result.RunningSyncViewModel;
                },
                total: function (result) {
                    var totalCount = result.RunningSyncViewModelTotalCount;
                    if (totalCount > 0) {
                        $("#runSyncCount").text(" (" + totalCount + ")");
                    }
                    return result.RunningSyncViewModelTotalCount;
                },
                model: {
                    fields: {
                        Id: { type: "string", editable: false, hidden: true },
                        Instance: { type: "string", editable: false },
                        SyncName: { type: "string", editable: false },
                        TableName: { type: "string", editable: false },
                        RecordsFound: { type: "string", editable: false },
                        RecordsInserted: { type: "string", editable: false },
                        RecordsUpdated: { type: "string", editable: false },
                        RecordsPosted: { type: "string", editable: false },
                        StartTime: { type: "string", editable: false }
                    }
                }
            },
            change: function (e) {

                if (e.sender._pageSize > 10) {
                    localStorage.setItem("runningSyncGridPageSize", e.sender._pageSize);
                } else {
                    localStorage.setItem("runningSyncGridPageSize", 10);
                }
                e.preventDefault();
            },
            sort: { field: "Instance", dir: "desc" },
            serverPaging: false,
            serverFiltering: false,
            serverSorting: false,
            pageSize: runningSyncDataSource.RunningSyncViewModelTotalCount
        },
        editable: true,
        columns: [
            {
                field: "Id",
                hidden: true
            },
            {
                field: "Instance",
                title: "Instance",
                template:
                    "#if (Instance != null) {# <div id='run_instance#=Id#'><span>#=Instance#</span></div> #} else" +
                        " { # <div id='run_instance_#=Id#'><span></span></div> # } #",
                width: 25,
                filterable: false,
                sortable: false
            }, {
                field: "SyncName",
                title: "Synchronization",
                template: "#if (SyncName != null) {# <div id='run_syncName#=Id#'><span>#=SyncName#</span></div> #} else" +
                    " { # <div id='run_syncName_#=Id#'><span></span></div> # } #",
                width: 50,
                filterable: false,
                sortable: true
            }, {
                field: "TableName",
                title: "TableName",
                template: "#if (TableName != null) {# <div id='run_tableName#=Id#'><span>#=TableName#</span></div> #} else" +
                    " { # <div id='run_tableName_#=Id#'><span></span></div> # } #",
                width: 90,
                filterable: false,
                sortable: true
            }, {
                field: "RecordsFound",
                title: "Snow Count",
                template: "#if (RecordsFound != null) {# <div id='run_recordsFound_#=Id#'><span>#=RecordsFound#</span></div> #} else" +
                    " { # <div id='run_recordsFound_#=Id#'><span></span></div> # } #",
                width: 40,
                filterable: false,
                sortable: true
            }, {
                field: "RecordsInserted",
                title: "Inserted",
                template: "#if (RecordsInserted != null) {# <div id='run_recordsInserted_#=Id#'><span>#=RecordsInserted#</span></div> #} else" +
                    " { # <div id='run_recordsInserted_#=Id#'><span></span></div> # } #",
                width: 30,
                filterable: false,
                sortable: true
            }, {
                field: "RecordsUpdated",
                title: "Updated",
                template: "#if (RecordsUpdated != null) {# <div id='run_recordsUpdated_#=Id#'><span>#=RecordsUpdated#</span></div> #} else" +
                    " { # <div id='run_recordsUpdated_#=Id#'><span></span></div> # } #",
                width: 30,
                filterable: false,
                sortable: true
            }, {
                field: "RecordsPosted",
                title: "Posted",
                template: "#if (RecordsPosted != null) {# <div id='run_recordsPosted_#=Id#'><span>#=RecordsPosted#</span></div> #} else" +
                    " { # <div id='run_recordsPosted_#=Id#'><span></span></div> # } #",
                width: 30,
                filterable: false,
                sortable: true
            }, {
                field: "StartTime",
                title: "Start Date",
                template: "#if (StartTime != null) {# <div id='run_startTime_#=Id#'><span>#=StartTime#</span></div> #} else" +
                    " { # <div id='run_startTime_#=Id#'><span></span></div> # } #",
                width: 40,
                filterable: false,
                sortable: true
            }
        ]
    }).data("kendoGrid");

    var runningSyncGrid = $("#runningSyncGrid").data("kendoGrid");
    runningSyncGrid.bind("dataBound", runningSyncGridDataBound);
    runningSyncGrid.dataSource.fetch();
}

function runningSyncGridDataBound(e) {

    var filter = this.dataSource.filter();
    this.thead.find(".k-header-column-menu.k-state-active").removeClass("k-state-active");
    if (filter) {
        var filteredMembers = {};
        setFilteredMembersDashboard(filter, filteredMembers);
        this.thead.find("th[data-field]").each(function () {
            var cell = $(this);
            var filtered = filteredMembers[cell.data("field")];
            if (filtered) {
                cell.find(".k-header-column-menu").addClass("k-state-active");
            }
        });
    }
}