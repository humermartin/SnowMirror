var selectedAdUserId;

$(document).ready(function () {

    $("#chkInterfaceMonitoring").bootstrapSwitch('size', 'small');
    $("#chkInterfaceMonitoring").bootstrapSwitch('onColor', 'success');
    $("#chkInterfaceMonitoring").bootstrapSwitch('offColor', 'primary');
    $("#chkInterfaceMonitoring").bootstrapSwitch('onText', 'Ein');
    $("#chkInterfaceMonitoring").bootstrapSwitch('offText', 'Aus');
    $("#chkInterfaceMonitoring")
        .bootstrapSwitch({
            onSwitchChange: function (e, state) {
                $("#chkInterfaceMonitoringSelected").val(state);
            }
        });

    $("#chkSynchronizationAlertNotify").bootstrapSwitch('size', 'small');
    $("#chkSynchronizationAlertNotify").bootstrapSwitch('onColor', 'success');
    $("#chkSynchronizationAlertNotify").bootstrapSwitch('offColor', 'primary');
    $("#chkSynchronizationAlertNotify").bootstrapSwitch('onText', 'Ein');
    $("#chkSynchronizationAlertNotify").bootstrapSwitch('offText', 'Aus');
    $("#chkSynchronizationAlertNotify")
        .bootstrapSwitch({
            onSwitchChange: function (e, state) {
                $("#chkSynchronizationAlertNotifySelected").val(state);
            }
        });

    $("#chkNotifyOnNotStartedSync").bootstrapSwitch('size', 'small');
    $("#chkNotifyOnNotStartedSync").bootstrapSwitch('onColor', 'success');
    $("#chkNotifyOnNotStartedSync").bootstrapSwitch('offColor', 'primary');
    $("#chkNotifyOnNotStartedSync").bootstrapSwitch('onText', 'Ein');
    $("#chkNotifyOnNotStartedSync").bootstrapSwitch('offText', 'Aus');
    $("#chkNotifyOnNotStartedSync")
        .bootstrapSwitch({
            onSwitchChange: function (e, state) {
                $("#chkNotifyOnNotStartedSyncSelected").val(state);
            }
        });
    
    $("#chkNotifyOnFailedSync").bootstrapSwitch('size', 'small');
    $("#chkNotifyOnFailedSync").bootstrapSwitch('onColor', 'success');
    $("#chkNotifyOnFailedSync").bootstrapSwitch('offColor', 'primary');
    $("#chkNotifyOnFailedSync").bootstrapSwitch('onText', 'Ein');
    $("#chkNotifyOnFailedSync").bootstrapSwitch('offText', 'Aus');
    $("#chkNotifyOnFailedSync")
        .bootstrapSwitch({
            onSwitchChange: function (e, state) {
                $("#chkNotifyOnFailedSyncSelected").val(state);
            }
        });

    $("#chkEnableKillSession").bootstrapSwitch('size', 'small');
    $("#chkEnableKillSession").bootstrapSwitch('onColor', 'success');
    $("#chkEnableKillSession").bootstrapSwitch('offColor', 'primary');
    $("#chkEnableKillSession").bootstrapSwitch('onText', 'Ein');
    $("#chkEnableKillSession").bootstrapSwitch('offText', 'Aus');
    $("#chkEnableKillSession")
        .bootstrapSwitch({
            onSwitchChange: function (e, state) {
                $("#chkEnableKillSessionSelected").val(state);
            }
        });

    
    $("#chkMailSendEnabled").bootstrapSwitch('size', 'medium');
    $("#chkMailSendEnabled").bootstrapSwitch('onColor', 'success');
    $("#chkMailSendEnabled").bootstrapSwitch('offColor', 'primary');
    $("#chkMailSendEnabled").bootstrapSwitch('onText', 'On');
    $("#chkMailSendEnabled").bootstrapSwitch('offText', 'Off');
    $("#chkMailSendEnabled")
        .bootstrapSwitch({
            onSwitchChange: function (e, state) {
                $("#mailSendEnabledSelected").val(state);
            }
        });

    $("#chkUseSSL").bootstrapSwitch('size', 'medium');
    $("#chkUseSSL").bootstrapSwitch('onColor', 'success');
    $("#chkUseSSL").bootstrapSwitch('offColor', 'primary');
    $("#chkUseSSL").bootstrapSwitch('onText', 'On');
    $("#chkUseSSL").bootstrapSwitch('offText', 'Off');
    $("#chkUseSSL")
        .bootstrapSwitch({
            onSwitchChange: function (e, state) {
                $("#useSslSelected").val(state);
            }
        });

    $("#chkAutomaticRetryProcessFullSync").bootstrapSwitch('size', 'small');
    $("#chkAutomaticRetryProcessFullSync").bootstrapSwitch('onColor', 'success');
    $("#chkAutomaticRetryProcessFullSync").bootstrapSwitch('offColor', 'primary');
    $("#chkAutomaticRetryProcessFullSync").bootstrapSwitch('onText', 'Ein');
    $("#chkAutomaticRetryProcessFullSync").bootstrapSwitch('offText', 'Aus');
    $("#chkAutomaticRetryProcessFullSync")
        .bootstrapSwitch({
            onSwitchChange: function (e, state) {
                $("#chkAutomaticRetryProcessFullSyncSelected").val(state);
            }
        });

    $("#chkAutomaticRetryProcessDeltaSync").bootstrapSwitch('size', 'small');
    $("#chkAutomaticRetryProcessDeltaSync").bootstrapSwitch('onColor', 'success');
    $("#chkAutomaticRetryProcessDeltaSync").bootstrapSwitch('offColor', 'primary');
    $("#chkAutomaticRetryProcessDeltaSync").bootstrapSwitch('onText', 'Ein');
    $("#chkAutomaticRetryProcessDeltaSync").bootstrapSwitch('offText', 'Aus');
    $("#chkAutomaticRetryProcessDeltaSync")
        .bootstrapSwitch({
            onSwitchChange: function (e, state) {
                $("#chkAutomaticRetryProcessDeltaSyncSelected").val(state);
            }
        });

    $("#chkEnableColumnSqlCount").bootstrapSwitch('size', 'small');
    $("#chkEnableColumnSqlCount").bootstrapSwitch('onColor', 'success');
    $("#chkEnableColumnSqlCount").bootstrapSwitch('offColor', 'primary');
    $("#chkEnableColumnSqlCount").bootstrapSwitch('onText', 'Ein');
    $("#chkEnableColumnSqlCount").bootstrapSwitch('offText', 'Aus');
    $("#chkEnableColumnSqlCount")
        .bootstrapSwitch({
            onSwitchChange: function (e, state) {
                $("#chkEnableColumnSqlCountSelected").val(state);
            }
        });

    $("#chkEnableColumnRecordCount").bootstrapSwitch('size', 'small');
    $("#chkEnableColumnRecordCount").bootstrapSwitch('onColor', 'success');
    $("#chkEnableColumnRecordCount").bootstrapSwitch('offColor', 'primary');
    $("#chkEnableColumnRecordCount").bootstrapSwitch('onText', 'Ein');
    $("#chkEnableColumnRecordCount").bootstrapSwitch('offText', 'Aus');
    $("#chkEnableColumnRecordCount")
        .bootstrapSwitch({
            onSwitchChange: function (e, state) {
                $("#chkEnableColumnRecordCountSelected").val(state);
            }
        });

    $("#chkEnableColumnSnowCount").bootstrapSwitch('size', 'small');
    $("#chkEnableColumnSnowCount").bootstrapSwitch('onColor', 'success');
    $("#chkEnableColumnSnowCount").bootstrapSwitch('offColor', 'primary');
    $("#chkEnableColumnSnowCount").bootstrapSwitch('onText', 'Ein');
    $("#chkEnableColumnSnowCount").bootstrapSwitch('offText', 'Aus');
    $("#chkEnableColumnSnowCount")
        .bootstrapSwitch({
            onSwitchChange: function (e, state) {
                $("#chkEnableColumnSnowCountSelected").val(state);
            }
        });


    $('#dialog').hide();
    
    $("#idManagementRoles").on("change", function () {
        if ($("#idManagementRoles").val().length !== 0 && $("#adUserName").val().length !== 0) {
            $("#btnRegisterAdUser").attr('disabled', false);
        } else {
            $("#btnRegisterAdUser").attr('disabled', true);
        }
    });
    
    function onClose() {
        $('#dialog').fadeIn();
    }

    $("#dialog").kendoDialog({
        width: "500px",
        title: "Are you sure removing the selected User?",
        closable: false,
        modal: false,
        content: "",
        actions: [
            { text: 'NO' },
            { text: 'OK', primary: true, action: onOk }
        ],
        close: onClose
    });

    
    $(".removeUserCredential")
        .hover(function () {
            $(this).css("cursor", "pointer");
        });

    if ($('#assignedAdUsersGrid').length > 0) {
        LoadAdUsersGrid();
    }

    if ($('#inheritedTablesGrid').length > 0) {
        LoadTableInheritanceGrid();
    }

    if ($('#notifyRecipientGrid').length > 0) {
        InitializeNotifyRecipientGrid();
    }

    if ($('#notifyTableSchemaChangeGrid').length > 0) {
        InitializeNotifyTableSchemaChange();
    }

    $("#inheritedTablesGrid").on("click", "#deleteTableInheritance", function (e) {
        e.preventDefault();
        var row = $(this).closest("tr");
        var grid = $("#inheritedTablesGrid").data("kendoGrid");
        var selectedItem = grid.dataItem(row);
        DeleteTableInheritance(selectedItem.ParentTable);
    });

    $("#notifyRecipientGrid").on("click", "#deleteNotifyRecipient", function (e) {
        e.preventDefault();
        var row = $(this).closest("tr");
        var grid = $("#notifyRecipientGrid").data("kendoGrid");
        var selectedItem = grid.dataItem(row);
        DeleteNotifyRecipient(selectedItem.EmailAddress);
    });

    $("#notifyTableSchemaChangeGrid").on("click", "#deleteSchemaChangeNotifyRecipient", function (e) {
        e.preventDefault();
        var row = $(this).closest("tr");
        var grid = $("#notifyTableSchemaChangeGrid").data("kendoGrid");
        var selectedItem = grid.dataItem(row);
        DeleteSchemaChangeNotifyRecipient(selectedItem.EmailAddress);
    });

    $("#syncTargetname").on("blur", function () {
        var selectedSyncTargetId = $("#SyncTargetSettings option:selected").val();
        if (selectedSyncTargetId === "") {
            ValidateSyncTargetName($("#syncTargetname").val());
        }
    });

    $("#updateSyncTarget").on("click", function (e) {
        var selectedSyncTargetId = $("#SyncTargetSettings option:selected").val();
        UpdateSyncTarget(selectedSyncTargetId);
    });

    $("#saveNewSyncTarget").on("click", function (e) {
        AddNewSyncTarget();
    });
    
    //set synctarget control
    SetSyncTargetControls();
});

function onOk(e) {
    RemoveAdUser(selectedAdUserId);
}

function addNewDatabase() {
    document.getElementById("Databases").selectedIndex = "0";
    document.getElementById("DatabaseID").value = "";
    document.getElementById("Servername").value = "";
    document.getElementById("Port").value = "";
    document.getElementById("Instancename").value = "";
    document.getElementById("Databasename").value = "";
    document.getElementById("Schemaname").value = "";
    document.getElementById("Username").value = "";
    document.getElementById("Password").value = "";
    document.getElementById("Update").setAttribute("hidden", "true");
    document.getElementById("Remove").setAttribute("hidden", "true");
    document.getElementById("TestConnection").setAttribute("hidden", "true");
    document.getElementById("connSucces").setAttribute("hidden", "true");
}

function SetDatabaseValues() {
    var e = document.getElementById("Databases");
    var strUser = e.options[e.selectedIndex].value;
    var res = strUser.split(";");
    if (res[1] === undefined) {
        document.getElementById("DatabaseId").value = "";
        document.getElementById("Servername").value = "";
        document.getElementById("Port").value = "";
        document.getElementById("Instancename").value = "";
        document.getElementById("Databasename").value = "";
        document.getElementById("Schemaname").value = "";
        document.getElementById("Username").value = "";
        document.getElementById("Password").value = "";
        document.getElementById("Update").setAttribute("hidden", "true");
        document.getElementById("Remove").setAttribute("hidden", "true");
        document.getElementById("TestConnection").setAttribute("hidden", "true");
        document.getElementById("connSucces").setAttribute("hidden", "true");
    } else {
        document.getElementById("DatabaseId").value = res[1].toUpperCase();
        document.getElementById("Update").removeAttribute("hidden");
        document.getElementById("Remove").removeAttribute("hidden");
        document.getElementById("TestConnection").removeAttribute("hidden");
        document.getElementById("connSucces").setAttribute("hidden", "true");
        $.ajax({
            type: "Get",
            url: "../Manage/GetDatabaseSettingsInfos",
            data: { databaseId: document.getElementById("DatabaseId").value },
            dataType: "json",
            success: function (data) {
                $("#DatabaseId").val(data[0].toUpperCase());
                $("#Servername").val(data[1]);
                $("#Port").val(data[2]);
                $("#Instancename").val(data[3]);
                $("#Databasename").val(data[4]);
                $("#Schemaname").val(data[5]);
                $("#Username").val(data[6]);
                $("#Password").val(data[7]);
            }
        });
    }
};

function addNewSnowInstance() {
    document.getElementById("snowInstanceId").value = "";
    document.getElementById("snowInstanceHost").value = "";
    document.getElementById("snowServername").value = "";
    document.getElementById("snowPort").value = "";
    document.getElementById("snowUserName").value = "";
    document.getElementById("snowPW").value = "";
    document.getElementById("snowProxyHost").value = "";
    document.getElementById("snowProxyPort").value = "";
    document.getElementById("snowProxyUserName").value = "";
    document.getElementById("snowProxyPW").value = "";
    document.getElementById("Update").setAttribute("hidden", "true");
    document.getElementById("Remove").setAttribute("hidden", "true");
    document.getElementById("TestConnection").setAttribute("hidden", "true");
    document.getElementById("connSucces").setAttribute("hidden", "true");
}

function SetInstanceValues() {
    var e = document.getElementById("Instances");
    var strUser = e.options[e.selectedIndex].value;
    var res = strUser.split(";");
    if (res[1] === undefined) {
        document.getElementById("snowInstanceId").value = "";
        document.getElementById("snowInstanceHost").value = "";
        document.getElementById("snowServername").value = "";
        document.getElementById("snowPort").value = "";
        document.getElementById("snowUserName").value = "";
        document.getElementById("snowPW").value = "";
        document.getElementById("snowProxyHost").value = "";
        document.getElementById("snowProxyPort").value = "";
        document.getElementById("snowProxyUserName").value = "";
        document.getElementById("snowProxyPW").value = "";
        document.getElementById("Update").setAttribute("hidden", "true");
        document.getElementById("Remove").setAttribute("hidden", "true");
        document.getElementById("TestConnection").setAttribute("hidden", "true");
        document.getElementById("connSucces").setAttribute("hidden", "true");
    } else {
        document.getElementById("snowInstanceID").value = res[1].toUpperCase();
        document.getElementById("Update").removeAttribute("hidden");
        document.getElementById("Remove").removeAttribute("hidden");
        document.getElementById("TestConnection").removeAttribute("hidden");
        document.getElementById("connSucces").setAttribute("hidden", "true");
        $.ajax({
            type: "Get",
            url: "../Manage/GetServiceNowInstanceSettingsInfos",
            data: { snowInstanceID: document.getElementById("snowInstanceID").value },
            dataType: "json",
            success: function (data) {
                $("#snowInstanceId").val(data[0].toUpperCase());
                $("#snowInstanceHost").val(data[1]);
                $("#snowServername").val(data[2]);
                $("#snowPort").val(data[3]);
                $("#snowUserName").val(data[4]);
                $("#snowPW").val(data[5]);
                $("#snowProxyHost").val(data[6]);
                $("#snowProxyPort").val(data[7]);
                $("#snowProxyUserName").val(data[8]);
                $("#snowProxyPW").val(data[9]);
            }
        });
    }
}

function testDbConn() {
    var sname = document.getElementById("Servername").value;
    var db = document.getElementById("Databasename").value;
    $.ajax({
        type: "Get",
        url: "../Manage/TestDatabaseConnection",
        data: { server: sname, dbName: db },
        dataType: "json",
        beforeSend: function () {
            $(".success-popup").show();
        },
        success: function (data) {
            document.getElementById("connSucces").removeAttribute("hidden");
            document.getElementById("connSucces").innerHTML = data;
        },
        complete: function () {
            $(".success-popup").hide();
        }
    });
};

function testSnowConn() {
    var instance = document.getElementById("snowInstanceHost").value;
    var u = document.getElementById("snowUserName").value;
    var pw = document.getElementById("snowPW").value;;
    $.ajax({
        type: "Get",
        url: "../Manage/TestSnowConnection",
        data: { instanceName: instance, user: u, password: pw },
        dataType: "json",
        beforeSend: function () {
            $(".success-popup").show();
        },
        success: function (data) {
            document.getElementById("connSucces").removeAttribute("hidden");
            document.getElementById("connSucces").innerHTML = data;
        },
        complete: function () {
            $(".success-popup").hide();
        }
    });
};

//search active directory user api
function SearchAdUser() {

    if ($('#adUserName').val() !== undefined) {
        
        $.ajax({
            type: "GET",
            url: "~/../../api/SnowApi/ValidateAdUser",
            data: {
                adUser: $('#adUserName').val()
            },
            dataType: "json",
            success: function (response) {
                if (response.ValidateUIDResult === true) {
                    $("#idSamAccountname").val(response.PrincipalModel.SamAccountName);
                    $("#idFirstName").val(response.PrincipalModel.FirstName);
                    $("#idLastName").val(response.PrincipalModel.LastName);

                    if ($("#idSamAccountname").val().length !== 0 && $("#idManagementRoles").val().length !== 0) {
                        $("#btnRegisterAdUser").attr('disabled', false);
                    }

                } else {
                    $("#validateAdUserMessage").text(response.Message);
                }
            },
            failure: function (xhr, status) {
                alertify.error(status + " - " + xhr.responseText);
            }
        });
    }
}

//add adUser for SnowSyncDbWeb
function AddAdUser() {

    if ($('#adUserName').val() !== "" && $("#idManagementRoles").val() !== "") {
        
        $.ajax({
            type: "GET",
            url: "~/../../api/SnowApi/RegisterPrincipals",
            data: {
                adUser: $('#adUserName').val(),
                managementRole: $('#idManagementRoles').val()
            },
            dataType: "json",
            success: function (response) {
                if (response.AddAdUserResult === false) {
                    $("#validateAdUserMessage").text(response.Message);
                } else {
                    alertify.success("User " + $('#adUserName').val() + " added successfully.");
                    LoadAdUsersGrid();
                }
            },
            failure: function (xhr, status) {
                alertify.error(status + " - " + xhr.responseText);
            }
        });
    }

}

//upade adUser activation
function UpdateAdUserActivation(principalId, active) {

    if (principalId !== "" && active !== null) {
        
        $.ajax({
            type: "GET",
            url: "~/../../api/SnowApi/UpdateAdUserActivation",
            data: { principalId: principalId, active: active },
            dataType: "json",
            success: function (response) {
                if (response.UpdateActivationResult === false) {
                    $('#validateAdUserListMessage').addClass('text-danger').removeClass('text-success');
                } else {
                    $('#validateAdUserListMessage').addClass('text-success').removeClass('text-danger');
                }
                $("#validateAdUserListMessage").text(response.Message);
            },
            failure: function (xhr, status) {
                alertify.error(status + " - " + xhr.responseText);
            }
        });
    }
}

//remove adUser from MirrorWebSync
function RemoveAdUser(principalId) {

    if (principalId !== "") {
        
        $.ajax({
            type: "GET",
            url: "RemoveAdUserAccount/Manage",
            data: { principalId: principalId },
            dataType: "json",
            success: function (response) {
                if (response.RemoveAdUserResult === false) {
                    $("#validateAdUserListMessage").text(response.Message);
                }
                window.location.href = response.RedirectUrl;
            },
            failure: function (xhr, status) {
                alertify.error(status + " - " + xhr.responseText);
            }
        });
    }

}

//initialize user administration overview
function LoadAdUsersGrid() {
    $("#assignedAdUsersGrid").css("display", "block");

    $("#assignedAdUsersGrid").kendoGrid({
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
        filterMenuInit: initFilterMenuSyncAdminSettings,
        pageable: {
            refresh: true,
            pageSizes: [5, 10],
            previousNext: true,
            width: 150,
            buttonCount: 3,
            messages: {
                display: "{0} - {1} of {2} Users",
                itemsPerPage: "Users per Page",
                empty: "No Data",
                allPages: "All"
            }
        },
        dataSource:
        {
            transport: {
                read: {
                    url: "~/../../api/SnowApi/InitializeAdUsersGrid",
                    dataType: "json",
                    contentType: "application/json",
                    type: 'GET',
                    data: {
                        
                    }
                },
                parameterMap: function (data, type) {
                    if (type === "read") {

                        return {
                            
                        };
                    }
                }
            },
            schema: {
                data: function (result) {
                    return result.Principals;
                },
                total: function (result) {
                    return result.PrincipalsTotalCount;
                },
                model: {
                    fields: {
                        UserName: { type: "string", editable: false },
                        FullName: { type: "string", editable: false },
                        RoleName: { type: "string", editable: true },
                        Active: { type: "boolean", editable: false },
                        CreatedTime: { type: "string", editable: false }
                        
                    }
                }
            },
            
            serverPaging: false,
            serverFiltering: false,
            serverSorting: true
        },
        editable: true,
        columns: [
            {
                field: "UserName",
                title: "AD User",
                width: 50,
                filterable: false,
                sortable: false
            }, {
                field: "FullName",
                title: "Name",
                width: 70,
                filterable: false,
                sortable: false
            }, {
                field: "RoleName",
                title: "Role",
                editor: mgntRoleDropDownEditor,
                template: "#= ManagementRole.RoleName#",
                width: 100,
                filterable: false,
                sortable: false
            }, {
                title: "Active",
                template: '<input id="#=Id #" type="checkbox" #= Active ? \'checked="checked"\' : "" # class="k-checkbox" onclick="SeAdUserActivity($(this));" style="padding: 0;"><label class="k-checkbox-label" for="#=Id #" style="font-weight: normal; text-align:center; padding: 0"></label>', 
                width: 20,
                filterable: false,
                sortable: false
            }, {
                field: "CreatedTime",
                title: "Created",
                width: 50,
                filterable: false,
                sortable: false
            }, {
                width: 30,
                template: '<button class="k-button" id="#=Id#" style="font-size: 12px;" onclick="RemodeAdUser($(this))">Remove</button>',
                title: "Remove"
            }
        ]
    }).data("kendoGrid");

    var adUsersGrid = $("#assignedAdUsersGrid").data("kendoGrid");
    adUsersGrid.bind("dataBound", adUsersGridDataBound);
    adUsersGrid.dataSource.fetch();
}

//Update User activation
function SeAdUserActivity(object) {
    var selItem = object[0];
    UpdateAdUserActivation(selItem.id, selItem.checked);
}

//Remove AdUSer
function RemodeAdUser(object) {
    var selItem = object[0];

    var dialog = $('#dialog');
    dialog.data("kendoDialog").open();
    selectedAdUserId = selItem.id;
}

//Set active/inactive filter forecolor 
function adUsersGridDataBound(e) {

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

function mgntRoleDropDownEditor(container, options) {
    $.ajax({
        type: "GET",
        url: "~/../../api/SnowApi/GetManagementRoles",
        data: {},
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            var model = result;

            if (model !== null && model !== undefined && model !== "") {
                $('<input id="roleDropDown" required name="' + options.field + ' "/>')
                    .appendTo(container)
                    .kendoDropDownList({
                        autoBind: false,
                        change: OnRoleChange,
                        dataTextField: "Text",
                        dataValueField: "Value",
                        dataSource: model.ManagementRoles
                    }).width(150);
            }
        },
        failure: function (xhr, status) {
            alertify.error(status + " - " + xhr.responseText);
        }
    });

}

//Update Role
function OnRoleChange(e) {
    var grid = e.sender.element.closest(".k-grid").data("kendoGrid");
    var row = e.sender.element.closest("tr");
    var dataItem = grid.dataItem(row);

    var selectedRole = e.sender._cascadedValue;

    $.ajax({
        type: "GET",
        url: "~/../../api/SnowApi/UpdateAdUserRole",
        data: { principalId: dataItem.Id, roleId: selectedRole},
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            if (response.UpdateAdUserRoleResult === false) {
                $('#validateAdUserListMessage').addClass('text-danger').removeClass('text-success');
            } else {
                $('#validateAdUserListMessage').addClass('text-success').removeClass('text-danger');
                LoadAdUsersGrid();
            }
            $("#validateAdUserListMessage").text(response.Message);
        },
        failure: function (xhr, status) {
            alertify.error(status + " - " + xhr.responseText);
        }
    });
}

//initialize user administration overview
function LoadTableInheritanceGrid() {

    $("#inheritedTablesGrid").kendoGrid({
        groupable: true,
        sortable: true,
        resizable: false,
        scrollable: true,
        height: 600,
        autoBind: false,
        toolbar: [
            {
                template: ' <div class="input-group"><div class="input-group-prepend" style="width: 100%"><span class="input-group-text" style="font-size: 12px;">Enter parent table</span></div><input type="text" id="parentTableName" class="form-control" style="font-size: 12px; width: 500px"></div><br/><div class="input-group"><div class="input-group-prepend" style="width: 100%"><span class="input-group-text" style="font-size: 12px;">Enter tables the are inherit from parent. Comma separated</span></div><input type="text" id="childTableName" class="form-control" style="font-size: 12px; width: 100% !important"></div><br /><a id="addInheritRecordId" class="btn btn-sm custHeaderBackground" href="\\#" onclick="return AddTableInheritRecord()" style="font-size: 12px; color: white; width: 50px;">Add</a>'
            }
        ],
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
        filterMenuInit: initFilterMenuSyncAdminSettings,
        pageable: {
            refresh: true,
            pageSizes: [10, 20, 50],
            previousNext: true,
            width: 160,
            buttonCount: 3,
            messages: {
                display: "{0} - {1} of {2} Tables(s)",
                itemsPerPage: "Tables per Page",
                empty: "No Data",
                allPages: "All"
            }
        },
        dataSource:
        {
            transport: {
                read: {
                    url: "~/../../api/SnowApi/InitializeTableInheritanceGrid",
                    dataType: "json",
                    contentType: "application/json",
                    type: 'GET',
                    data: {
                        
                    }
                },
                parameterMap: function (data, type) {
                    if (type === "read") {

                        return {
                            
                        };
                    }
                }
            },
            schema: {
                data: function (result) {
                    return result.SnowTableInheritance;
                },
                total: function (result) {
                    return result.SnowTableInheritanceTotalCount;
                },
                model: {
                    fields: {
                        Id: { type: "string", editable: false },
                        ParentTable: { type: "string", editable: false },
                        ChildTables: { type: "string", editable: false }
                    }
                }
                
            },
            serverPaging: false,
            serverFiltering: false,
            serverSorting: true
        },
        editable: true,
        columns: [
            {
                field: "Id",
                title: "Id",
                hidden: true
            }, {
                field: "ParentTable",
                title: "Parent Table",
                width: 40
            }, {
                field: "ChildTables",
                title: "Child Tables",
                width: 150
            }, {
                width: 30,
                template: "<button class='k-button' id='deleteTableInheritance'>Delete</button>",
                title: "Remove"
            }
        ]
    }).data("kendoGrid");

    var tableInheritanceGrid = $("#inheritedTablesGrid").data("kendoGrid");
    tableInheritanceGrid.bind("dataBound", tableInheritanceGridDataBound);
    tableInheritanceGrid.dataSource.fetch();
}

//Set active/inactive filter forecolor 
function tableInheritanceGridDataBound(e) {

    var filter = this.dataSource.filter();
    this.thead.find(".k-header-column-menu.k-state-active").removeClass("k-state-active");
    if (filter) {
        var filteredMembers = {};
        setTableInheritanceFilteredMembers(filter, filteredMembers);
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
function setTableInheritanceFilteredMembers(filter, members) {
    if (filter.filters) {
        for (var i = 0; i < filter.filters.length; i++) {
            setFilteredMembers(filter.filters[i], members);
        }
    }
    else {
        members[filter.field] = true;
    }
}

function AddTableInheritRecord() {
    var parentTableName = $("#parentTableName").val();
    var childTableName = $("#childTableName").val();
    
    $.ajax({
        type: "GET",
        url: "~/../../api/SnowApi/AddNewInheritance",
        data: { parentTableName: parentTableName, childTableNames: childTableName },
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (result) {
            //reload grid
            LoadTableInheritanceGrid();

        },
        failure: function (xhr, status) {
            alertify.error(status + " - " + xhr.responseText);
        }
    });

}

//delete table inheritance
function DeleteTableInheritance(parentTableName) {
    
    $.ajax({
        type: "GET",
        url: "~/../../api/SnowApi/DeleteTableInheritance",
        dataType: "json",
        contentType: "application/json; charset=utf8",
        data: { parentTableName: parentTableName },
        success: function (res) {
            //reload grid
            LoadTableInheritanceGrid();
        },
        error: function (xhr, status, error) {
            var err = eval("(" + xhr.responseText + ")");
            alertify.error(err);
        }
    });
}

//Load notify recipient
function InitializeNotifyRecipientGrid() {
    $("#notifyRecipientGrid").css("display", "block");

    $("#notifyRecipientGrid").kendoGrid({
        groupable: true,
        sortable: true,
        resizable: false,
        scrollable: true,
        height: 600,
        autoBind: false,
        toolbar: [
            {
                template: '<div style="text-align: center;"><div class="input-group"><div class="input-group-prepend"><span class="input-group-text" style="font-size: 12px;">Enter notify recipient name</span></div><input id="emailNameId" class="k-textbox" style="font-size: 12px; width: 250px; height: 35px;"></input></div><div class="input-group"><div class="input-group-prepend"><span class="input-group-text" style="font-size: 12px;">Enter notify recipient address</span></div><input id="emailAddressId" class="k-textbox" style="font-size: 12px; width: 250px; height: 35px;"></input></div><br /><a id="emailAddressId" class="btn btn-sm custHeaderBackground" href="\\#" onclick="return AddNotifyRecipient()" style="font-size: 12px; color: white; width:150px; line-height: 30px;">Add Recipient</a></div>'
            }
        ],
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
        filterMenuInit: initFilterMenuSyncAdminSettings,
        pageable: {
            refresh: true,
            pageSizes: [10, 20, 50],
            previousNext: true,
            width: 160,
            buttonCount: 3,
            messages: {
                display: "{0} - {1} of {2} Script(s)",
                itemsPerPage: "Email per Page",
                empty: "No Data",
                allPages: "All"
            }
        },
        dataSource:
        {
            transport: {
                read: {
                    url: "~/../../api/SnowApi/InitializeNotifyRecipientGrid",
                    dataType: "json",
                    contentType: "application/json",
                    type: 'GET',
                    data: {
                    }
                },
                parameterMap: function (data, type) {
                    if (type === "read") {

                        return {
                        };
                    }
                }
            },
            schema: {
                data: function (result) {
                    return result.EmailRecipients;
                },
                total: function (result) {
                    return result.EmailRecipientsTotalCount;
                },
                model: {
                    fields: {
                        Id: { type: "string", editable: false },
                        Name: { type: "string", editable: false },
                        EmailAddress: { type: "string", editable: false }
                    }
                }
            },
            change: function (e) {
                
                e.preventDefault();
            },

            serverPaging: false,
            serverFiltering: false,
            serverSorting: true
        },
        editable: true,
        columns: [
            {
                field: "Id",
                title: "Id",
                hidden: true
            }, {
                field: "Name",
                title: "Name",
                width: 60
            }, {
                field: "EmailAddress",
                title: "EmailAddress",
                width: 150
            }, {
                width: 30,
                template: "<button class='k-button' id='deleteNotifyRecipient'>Delete</button>",
                title: "Remove"
            }
        ]
    }).data("kendoGrid");

    var notifyRecipientGrid = $("#notifyRecipientGrid").data("kendoGrid");
    notifyRecipientGrid.bind("dataBound", notifyRecipientGridDataBound);
    notifyRecipientGrid.dataSource.fetch();
}

//init filter
function initFilterMenuSyncAdminSettings(e) {
    var firstDropDown = $('[data-bind="value: filters[0].operator"]').data('kendoDropDownList');
    $('button[type="submit"]').click(function (ev) {

        var fieldType = getGridColumnFieldType(e.sender.dataSource, e.field);

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

//Set active/inactive filter forecolor 
function notifyRecipientGridDataBound(e) {

    var filter = this.dataSource.filter();
    this.thead.find(".k-header-column-menu.k-state-active").removeClass("k-state-active");
    if (filter) {
        var filteredMembers = {};
        setFilteredMembersSyncSettings(filter, filteredMembers);
        this.thead.find("th[data-field]").each(function () {
            var cell = $(this);
            var filtered = filteredMembers[cell.data("field")];
            if (filtered) {
                cell.find(".k-header-column-menu").addClass("k-state-active");
            }
        });
    }
}


function AddNotifyRecipient() {
    
    if ($('#emailNameId').val() !== "" && $("#emailAddressId").val() !== "") {
        $.ajax({
            type: "GET",
            url: "~/../../api/SnowApi/AddNotifyRecipient",
            data: {
                emailName: $('#emailNameId').val(),
                emailAddress: $('#emailAddressId').val()
            },
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (result) {
                //reload grid
                InitializeNotifyRecipientGrid();

            },
            failure: function (xhr, status) {
                alertify.error(status + " - " + xhr.responseText);
            }
        });
    }
}

//delete notifyRecipient
function DeleteNotifyRecipient(notifyRecipient) {

    $.ajax({
        type: "GET",
        url: "~/../../api/SnowApi/DeleteNotifyRecipient",
        dataType: "json",
        contentType: "application/json; charset=utf8",
        data: { notifyRecipient: notifyRecipient },
        success: function (res) {
            //reload grid
            InitializeNotifyRecipientGrid();
        },
        error: function (xhr, status, error) {
            var err = eval("(" + xhr.responseText + ")");
            alertify.error(err);
        }
    });
}

//init grid for schema change notifications
function InitializeNotifyTableSchemaChange() {
    $("#notifyTableSchemaChangeGrid").css("display", "block");

    $("#notifyTableSchemaChangeGrid").kendoGrid({
        groupable: true,
        sortable: true,
        resizable: false,
        scrollable: true,
        height: 600,
        autoBind: false,
        toolbar: [
            {
                template: '<div style="text-align: center;"><div class="input-group"><div class="input-group-prepend"><span class="input-group-text" style="font-size: 12px;">Enter notify recipient name</span></div><input id="schemaChangeEmailNameId" class="k-textbox" style="font-size: 12px; width: 250px; height: 35px;"></input></div><div class="input-group"><div class="input-group-prepend"><span class="input-group-text" style="font-size: 12px;">Enter notify recipient address</span></div><input id="schemaChangeEmailAddressId" class="k-textbox" style="font-size: 12px; width: 250px; height: 35px;"></input></div><br /><a id="schemaChangeEmailAddressId" class="btn btn-sm custHeaderBackground" href="\\#" onclick="return AddSchemaChangeNotifyRecipient()" style="font-size: 12px; color: white; width:150px; line-height: 30px;">Add Recipient</a></div>'
            }
        ],
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
        filterMenuInit: initFilterMenuSyncAdminSettings,
        pageable: {
            refresh: true,
            pageSizes: [10, 20, 50],
            previousNext: true,
            width: 160,
            buttonCount: 3,
            messages: {
                display: "{0} - {1} of {2} Script(s)",
                itemsPerPage: "Email per Page",
                empty: "No Data",
                allPages: "All"
            }
        },
        dataSource:
        {
            transport: {
                read: {
                    url: "~/../../api/SnowApi/InitializeSchemaChangeNotifyGrid",
                    dataType: "json",
                    contentType: "application/json",
                    type: 'GET',
                    data: {
                    }
                },
                parameterMap: function (data, type) {
                    if (type === "read") {

                        return {
                        };
                    }
                }
            },
            schema: {
                data: function (result) {
                    return result.EmailRecipients;
                },
                total: function (result) {
                    return result.EmailRecipientsTotalCount;
                },
                model: {
                    fields: {
                        Id: { type: "string", editable: false },
                        Name: { type: "string", editable: false },
                        EmailAddress: { type: "string", editable: false }
                    }
                }
            },
            change: function (e) {

                e.preventDefault();
            },

            serverPaging: false,
            serverFiltering: false,
            serverSorting: true
        },
        editable: true,
        columns: [
            {
                field: "Id",
                title: "Id",
                hidden: true
            }, {
                field: "Name",
                title: "Name",
                width: 60
            }, {
                field: "EmailAddress",
                title: "EmailAddress",
                width: 150
            }, {
                width: 30,
                template: "<button class='k-button' id='deleteSchemaChangeNotifyRecipient'>Delete</button>",
                title: "Remove"
            }
        ]
    }).data("kendoGrid");

    var notifyTableSchemaChangeGrid = $("#notifyTableSchemaChangeGrid").data("kendoGrid");
    notifyTableSchemaChangeGrid.bind("dataBound", notifyTableSchemaChangeGridDataBound);
    notifyTableSchemaChangeGrid.dataSource.fetch();
}

function notifyTableSchemaChangeGridDataBound(e) {

    var filter = this.dataSource.filter();
    this.thead.find(".k-header-column-menu.k-state-active").removeClass("k-state-active");
    if (filter) {
        var filteredMembers = {};
        setFilteredMembersSyncSettings(filter, filteredMembers);
        this.thead.find("th[data-field]").each(function () {
            var cell = $(this);
            var filtered = filteredMembers[cell.data("field")];
            if (filtered) {
                cell.find(".k-header-column-menu").addClass("k-state-active");
            }
        });
    }
}

function AddSchemaChangeNotifyRecipient() {

    if ($('#schemaChangeEmailNameId').val() !== "" && $("#schemaChangeEmailAddressId").val() !== "") {
        $.ajax({
            type: "GET",
            url: "~/../../api/SnowApi/AddSchemaChangeNotifyRecipient",
            data: {
                emailName: $('#schemaChangeEmailNameId').val(),
                emailAddress: $('#schemaChangeEmailAddressId').val()
            },
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (result) {
                //reload grid
                $('#schemaChangeEmailNameId').val("");
                $("#schemaChangeEmailAddressId").val("");
                InitializeNotifyTableSchemaChange();

            },
            failure: function (xhr, status) {
                alertify.error(status + " - " + xhr.responseText);
            }
        });
    }
}

//delete schema change notify recipient
function DeleteSchemaChangeNotifyRecipient(notifyRecipient) {

    $.ajax({
        type: "GET",
        url: "~/../../api/SnowApi/DeleteSchemaChangeNotifyRecipient",
        dataType: "json",
        contentType: "application/json; charset=utf8",
        data: { notifyRecipient: notifyRecipient },
        success: function (res) {
            //reload grid
            InitializeNotifyTableSchemaChange();
        },
        error: function (xhr, status, error) {
            var err = eval("(" + xhr.responseText + ")");
            alertify.error(err);
        }
    });
}

function SetSyncTargetControls() {

    $("#saveNewSyncTargetRow").hide();
    var selectedSyncTargetId = $("#SyncTargetSettings option:selected").val();
    if (selectedSyncTargetId === "") {
        $("#updateSyncTargetRow").hide();
    } else {
        $("#updateSyncTargetRow").show();
        SetSyncTargetValues();
    }
}

//set selected sync target values
function SetSyncTargetValues() {

    $("#saveNewSyncTargetRow").hide();

    var selectedSyncTargetId = $("#SyncTargetSettings option:selected").val();

    if (selectedSyncTargetId !== "") {
        GetSyncTarget(selectedSyncTargetId);
        $("#updateSyncTargetRow").show();
    } else {
        $("#updateSyncTargetRow").hide();
    }
    
}

//get sync target
function GetSyncTarget(selectedSyncTargetId) {

    if (selectedSyncTargetId !== "") {
        $.ajax({
            type: "Get",
            url: "~/../../api/SnowApi/GetSyncTargetById",
            data: { syncTargetId: selectedSyncTargetId },
            dataType: "json",
            success: function (data) {
                $("#syncTargetId").val(data.Id);
                $("#syncTargetType").val(data.TargetType);
                $("#syncTargetname").val(data.Targetname);
                $("#syncTargetEndpoint").val(data.Endpoint);
                $("#syncTargetUsername").val(data.Username);
                $("#syncTargetPassword").val(data.Password);

            }
        });
    }
}

//update sync target
function UpdateSyncTarget(selectedSyncTargetId) {

    var syncTargetObject = new Object();
    syncTargetObject.Id = selectedSyncTargetId;
    syncTargetObject.TargetType = $("#syncTargetType").val();
    syncTargetObject.Targetname = $("#syncTargetname").val();
    syncTargetObject.Endpoint = $("#syncTargetEndpoint").val();
    syncTargetObject.Username = $("#syncTargetUsername").val();
    syncTargetObject.Password= $("#syncTargetPassword").val();

    if (selectedSyncTargetId !== "") {
        $.ajax({
            type: "POST",
            url: "~/../../api/SnowApi/UpdateSyncTargetById",
            dataType: "json",
            contentType: "application/json; charset=utf8",
            data: JSON.stringify(syncTargetObject),
            success: function (res) {
                if (res.Success) {
                    alertify.success("SyncTarget update successfull");
                }
            },
            error: function (xhr, status, error) {
                var err = eval("(" + xhr.responseText + ")");
                alertify.error(err);
            }
        });
    }
}

//add and save new sync target
function AddNewSyncTarget() {

    var syncTargetObject = new Object();
    syncTargetObject.TargetType = $("#syncTargetType").val();
    syncTargetObject.Targetname = $("#syncTargetname").val();
    syncTargetObject.Endpoint = $("#syncTargetEndpoint").val();
    syncTargetObject.Username = $("#syncTargetUsername").val();
    syncTargetObject.Password = $("#syncTargetPassword").val();

    $.ajax({
        type: "POST",
        url: "~/../../api/SnowApi/AddSyncTarget",
        dataType: "json",
        contentType: "application/json; charset=utf8",
        data: JSON.stringify(syncTargetObject),
        success: function (res) {
            if (res.Success) {
                //$("#syncTargetId").val(res.Id);
                alertify.success("SyncTarget update successfull");
                window.location.href = res.RedirectUrl;
            }
        },
        error: function (xhr, status, error) {
            var err = eval("(" + xhr.responseText + ")");
            alertify.error(err);
        }
    });
    
}

//reset all fields to support adding new synctarget
function ResetSyncTargetFields() {
    $("#SyncTargetSettings").val("");
    $("#syncTargetId").val("");
    $("#syncTargetname").val("");
    $("#syncTargetEndpoint").val("");
    $("#syncTargetUsername").val("");
    $("#syncTargetPassword").val("");
    $("#updateSyncTargetRow").hide();
    $("#saveNewSyncTargetRow").show();
}

//validate target name
function ValidateSyncTargetName(newTargetName) {
    if (newTargetName !== "") {
        $.ajax({
            type: "Get",
            url: "~/../../api/SnowApi/ValidateTargetName",
            data: { syncTargetName: newTargetName },
            dataType: "json",
            success: function (res) {
                if (!res.Success) {
                    alertify.error("Targetname exists already. Please use another.");
                }
            }
        });
    }
}