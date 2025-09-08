var intervalId;

$(document).ready(function () {

    $("#loadDeltaSyncs").bind("click", function () {
        BuildMonitoring();
    });

    $("#loadNodesKit").bind("click", function () {
        LoadNodeStateMonitoring("a1kit");
    });

    $("#loadNodesInt").bind("click", function () {
        LoadNodeStateMonitoring("a1int");
    });

    $("#loadNodesProd").bind("click", function () {
        LoadNodeStateMonitoring("a1prod");
    });

    $("#loadServiceLogFile").bind("click", function () {
        LoadLogFile("Server");
    });

    $("#loadWebAppLogFile").bind("click", function () {
        LoadLogFile("WebApp");
    });
});


//build monitoring
function BuildMonitoring() {

    $("#deltaSyncMonitoring").show();

    var url = "~/../../api/SnowApi/GetMonitoringModel";
    //if (vPath != null || vPath != undefined || vPath != "") {
    //    url = "~/" + vPath + "/../../api/SnowApi/GetMonitoringModel";
    //}

    $.ajax({
        type: "GET",
        url: "~/../../api/SnowApi/GetMonitoringModel",
        dataType: "json",
        success: function (result) {
            BuildMonitoringGrid(result);
            $("#loadedInSeconds").text(result.LoadedInSeconds + " sec.");
        },
        error: function (xhr, status) {
            var err = JSON.parse(xhr.responseText);
            alertify.error(err.Message);
        }
    });
}

//Delta Grid
function BuildMonitoringGrid(monitoringDataSource) {
    $(".loader").css("display", "none");
    $("#monitorGrid").css("display", "block");

    $("#monitorGrid").kendoGrid({
        groupable: true,
        sortable: true,
        resizable: false,
        scrollable: true,
        height: 600,
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
        filterMenuInit: initFilterMonitoringGrid,
        pageable: false,
        dataSource:
        {
            transport: {
                read: function (options) {
                    options.success(monitoringDataSource);
                }
            },
            schema: {
                data: function (result) {
                    return result.TableRecords;
                },
                total: function (result) {
                    return result.TableRecordsTotalCount;
                },
                model: {
                    fields: {
                        Id: { type: "string", editable: false, hidden: true },
                        TableName: { type: "string", editable: false },
                        Instance: { type: "string", editable: false },
                        StartTime: { type: "date", editable: false },
                        EndTime: { type: "date", editable: false },
                        Duration: { type: "string", editable: false },
                        Period: { type: "string", editable: false },
                        GetDeltaRecordsFrom: { type: "date", editable: false }
                    }
                }
            },
            change: function (e) {

                if (e.sender._pageSize > 10) {
                    localStorage.setItem("monitoringGridPageSize", e.sender._pageSize);
                } else {
                    localStorage.setItem("monitoringGridPageSize", 10);
                }
                e.preventDefault();
            },
            group: [
                { field: "Instance" },
                { field: "TableName" }
            ],
            sort: {
                field: "StartTime",
                dir: "desc"
            },
            serverPaging: false,
            serverFiltering: false,
            serverSorting: false,
            pageSize: monitoringDataSource.SyncDeltaViewModelsTotalCount
        },
        editable: true,
        columns: [
            {
                field: "Id",
                hidden: true
            }, {
                field: "TableName",
                title: "TableName",
                width: 60,
                filterable: true,
                sortable: true
            }, {
                field: "Instance",
                title: "Instance",
                width: 30,
                filterable: true,
                sortable: true
            }, {
                field: "StartTime",
                title: "StartTime",
                format: "{0: yyyy-MM-dd HH:mm:ss}",
                width: 60,
                filterable: false,
                sortable: true
            }, {
                field: "EndTime",
                title: "EndTime",
                format: "{0: yyyy-MM-dd HH:mm:ss}",
                width: 60,
                filterable: false,
                sortable: true
            }, {
                field: "Duration",
                title: "Duration",
                width: 30,
                filterable: false,
                sortable: true
            }, {
                field: "Period",
                title: "Period",
                width: 25,
                filterable: false,
                sortable: true
            }, {
                field: "GetDeltaRecordsFrom",
                title: "GetDeltaRecordsFrom",
                format: "{0: yyyy-MM-dd HH:mm:ss}",
                width: 60,
                filterable: false,
                sortable: true
            
            }
        ]
    }).data("kendoGrid");

    var plannedDeltaSyncGrid = $("#monitorGrid").data("kendoGrid");
    plannedDeltaSyncGrid.bind("dataBound", monitoringGridDataBound);
    plannedDeltaSyncGrid.dataSource.fetch();
}

function monitoringGridDataBound(e) {

    var filter = this.dataSource.filter();
    this.thead.find(".k-header-column-menu.k-state-active").removeClass("k-state-active");
    if (filter) {
        var filteredMembers = {};
        setGeneralFilteredMembers(filter, filteredMembers);
        this.thead.find("th[data-field]").each(function () {
            var cell = $(this);
            var filtered = filteredMembers[cell.data("field")];
            if (filtered) {
                cell.find(".k-header-column-menu").addClass("k-state-active");
            }
        });
    }
}

//init filter
function initFilterMonitoringGrid(e) {
    var firstDropDown = $('[data-bind="value: filters[0].operator"]').data('kendoDropDownList');
    $('button[type="submit"]').click(function (ev) {

        //reset filter selection
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

//Set general filtered members
function setGeneralFilteredMembers(filter, members) {
    if (filter.filters) {
        for (var i = 0; i < filter.filters.length; i++) {
            setGeneralFilteredMembers(filter.filters[i], members);
        }
    }
    else {
        members[filter.field] = true;
    }
}

//load ServiceNOW nodes overview
function LoadNodeStateMonitoring(instance) {

    $("#nodeStateMonitoring").show();

    $.ajax({
        type: "GET",
        url: "~/../../api/SnowApi/GetSnowNodesModel",
        data: { instanceName: instance },
        dataType: "json",
        success: function (result) {
            BuildNodeMonitoringGrid(result, instance);
        },
        error: function (xhr, status) {
            var err = JSON.parse(xhr.responseText);
            alertify.error(err.Message);
        }
    });
}

function BuildNodeMonitoringGrid(sysClusterStateDataSource, instanceName) {
    $(".loader").css("display", "none");
    $("#nodesStateGrid").css("display", "block");
    $("#headerNodeOverview").text("Service NOW Nodes");
    $("#headerNodeInstance").text(instanceName);
    
    $("#nodesStateGrid").kendoGrid({
        sortable: true,
        resizable: false,
        scrollable: true,
        height: 600,
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
        filterMenuInit: initFilterMonitoringGrid,
        pageable: {
            refresh: true,
            pageSizes: [10, 20, 50, 100, 500, 1000],
            previousNext: true,
            width: 1006,
            buttonCount: 10,
            messages: {
                display: "{0} - {1} of {2} Tables",
                itemsPerPage: "Nodes per Page",
                empty: "No Data",
                allPages: "All"
            }
        },
        dataSource:
        {
            transport: {
                read: function (options) {
                    options.success(sysClusterStateDataSource);
                }
            },
            schema: {
                data: function (result) {
                    return result.Nodes;
                },
                total: function (result) {
                    return result.NodesTotalCount;
                },
                model: {
                    fields: {
                        sys_id: { type: "string", editable: false },
                        system_id: { type: "string", editable: false },
                        node_id: { type: "string", editable: false },
                        status: { type: "string", editable: false },
                        schedulers: { type: "string", editable: false },
                        build_name: { type: "string", editable: false },
                        allow_inbound: { type: "string", editable: false }
                    }
                }
            },
            change: function (e) {

                if (e.sender._pageSize > 10) {
                    localStorage.setItem("nodeGridPageSize", e.sender._pageSize);
                } else {
                    localStorage.setItem("nodeGridPageSize", 10);
                }
                e.preventDefault();
            },
            serverPaging: true,
            serverFiltering: true,
            serverSorting: false,
            pageSize: sysClusterStateDataSource.NodesTotalCount
        },
        editable: true,
        columns: [
            {
                field: "sys_id",
                title: "SysId",
                hidden: true
            }, {
                field: "system_id",
                title: "SystemId",
                width: 100,
                filterable: true,
                sortable: true
            }, {
                field: "node_id",
                title: "NodeId",
                width: 70,
                filterable: true,
                sortable: true
            }, {
                field: "status",
                title: "Status",
                width: 40,
                filterable: false,
                sortable: true
            }, {
                field: "schedulers",
                title: "Schedulers",
                width: 40,
                filterable: false,
                sortable: true
            
            }, {
                field: "build_name",
                title: "BuildName",
                width: 60,
                filterable: false,
                sortable: true
            }, {
                field: "allow_inbound",
                title: "AllowInbound",
                width: 30,
                filterable: false,
                sortable: true
            }, {
                width: 30,
                template: '<button class="k-button" name="#=system_id#" id="#=sys_id#" style="font-size: 12px;" onclick="GetNodeStats($(this))">Stats</button>',
                title: "Stats"
            }
        ]
    }).data("kendoGrid");

    var nodeOverviewGrid = $("#nodesStateGrid").data("kendoGrid");
    nodeOverviewGrid.bind("dataBound", monitoringGridDataBound);
    nodeOverviewGrid.dataSource.fetch();
}

function GetNodeStats(object) {
    var selItem = object[0];
    var instance = $("#headerNodeInstance").text();
    
    $.ajax({
        type: "GET",
        url: "~/../../api/SnowApi/GetNodeStats",
        data: { instanceName: instance, nodeId: selItem.id },
        dataType: "json",
        success: function (result) {
            
            $("#headerNodeParam").text(selItem.name);
            $("#nodeStateParams").show();

            $("#nodeStatus").addClass("fa fa-lightbulb-o");
            if (result.SystemStatus.Text === "running") {
                $("#nodeStatus").css('color', 'green');
            } else {
                $("#nodeStatus").css('color', 'red');
            }
            
            $("#schedulerSystemId").text(result.SchedulerSystemId);
            
            $("#loggedInUsers").text(result.Sessionsummary.LoggedIn);
            $("#memoryStat").text(result.SystemMemoryInUse + " from total " + result.SystemMemoryTotal);
            $("#schedulerIsRunning").addClass("fa fa-lightbulb-o");
            if (result.SchedulerIsRunning === true) {
                $("#schedulerIsRunning").css('color', 'green');
            } else {
                $("#schedulerIsRunning").css('color', 'red');
            }

            $("#schedulerQueueLength").text(result.SchedulerQueueLength);
            $("#servletCancelledTransactions").text(result.ServletCancelledTransactions);
            $("#servletTransactions").text(result.ServletTransactions);
            $("#servletErrorsHandled").text(result.ServletErrorsHandled);

        },
        error: function (xhr, status) {
            var err = JSON.parse(xhr.responseText);
            alertify.error(err.Message);
        }
    });

}

function LoadLogFile(fileType) {
    alert("Load logfile from " + fileType);

    $.ajax({
        type: "GET",
        url: "~/../../api/SnowApi/GetLogFile",
        data: { fileType: fileType },
        dataType: "json",
        success: function (result) {
            $("#showWebAppLogContent").show(); 
            document.getElementById("loggingWebApp").innerHTML = result;
        },
        error: function (xhr, status) {
            var err = JSON.parse(xhr.responseText);
            alertify.error(err.Message);
        }
    });
}