$(document)
    .ready(function () {

        $("#chkMonday")
            .bootstrapSwitch({
                onSwitchChange: function (e, state) {
                    $("#periodMondaySelected").val(state);
                }
            });

        $("#chkTuesday")
            .bootstrapSwitch({
                onSwitchChange: function (e, state) {
                    $("#periodTuesdaySelected").val(state);
                }
            });

        $("#chkWednesday")
            .bootstrapSwitch({
                onSwitchChange: function (e, state) {
                    $("#periodWednesdaySelected").val(state);
                }
            });

        $("#chkThursday")
            .bootstrapSwitch({
                onSwitchChange: function (e, state) {
                    $("#periodThursdaySelected").val(state);
                }
            });

        $("#chkFriday")
            .bootstrapSwitch({
                onSwitchChange: function (e, state) {
                    $("#periodFridaySelected").val(state);
                }
            });

        $("#chkSaturday")
            .bootstrapSwitch({
                onSwitchChange: function (e, state) {
                    $("#periodSaturdaySelected").val(state);
                }
            });

        $("#chkSunday")
            .bootstrapSwitch({
                onSwitchChange: function (e, state) {
                    $("#periodSundaySelected").val(state);
                }
            });

        $("#syncScheduleDatePicker, #custDeltaStartPicker").kendoDateTimePicker({
            timeFormat: "HH:mm:ss",
            format: "dd.MM.yyyy HH:mm:ss",
            parseFormats: ["dd.MM.yyyy hh:mm:sstt", "dd.MM.yyyy HH:mm:ss", "dd.MM.yyyy", "HH:mm:ss"]
        });

        $("#syncScheduleTimePicker").kendoTimePicker({
            timeInput: true,
            format: 'HH:mm'
        });

        
        SetTargetControl();

        $("#targetType").bind("change", function () {

            SetTargetControl();
        });
        
        PreRenderScheduler();
    });

function PreRenderScheduler() {
    
    var interval = $("#syncIntervalId option:selected").val();
    
    if (interval === "0") {
        //0 = empty
        $("#schedulerActiveSince").hide();
        $("#schedulerTime").hide();
        $("#schedulerDaysOfWeek").hide();
        $("#schedulerIntervalInMinutes").hide();
    }else if (interval === "1") {
        //1 = daily
        $("#schedulerActiveSince").show();
        $("#schedulerTime").show();
        $("#schedulerDaysOfWeek").hide();
    } else if (interval === "2" || interval === "5" || interval === "6" || interval === "7" || interval === "8") {
        //2 = weekly, 5 = twoWeeks, 6 = threeWeeks, 7 = fourWeeks, 8 = fiveWeeks,
        $("#schedulerActiveSince").show();
        $("#schedulerDaysOfWeek").show();
        $("#schedulerTime").show();
    } else if (interval === "3") {
        //3 = periodically
        $("#schedulerActiveSince").show();
        $("#schedulerDaysOfWeek").show();
        $("#schedulerTime").show();
        $("#schedulerIntervalInMinutes").show();
        
    } else if (interval === "4") {
        //4 = manual
        $("#schedulerActiveSince").hide();
        $("#schedulerTime").hide();
        $("#schedulerDaysOfWeek").hide();
        $("#schedulerIntervalInMinutes").hide();
    } else if (interval === "9") {
        //4 = manual
        $("#schedulerActiveSince").hide();
        $("#schedulerTime").show();
        $("#schedulerDaysOfWeek").hide();
        $("#schedulerIntervalInMinutes").hide();
    }

    var e = document.getElementById("syncTypeId");
    if (e.options[e.selectedIndex].text === "Delta") {
        $("#custDeltaStart").show();
        $("#syncScheduleSubtractFromMinutesId").show();
    } else {
        $("#custDeltaStart").hide();
        $("#syncScheduleSubtractFromMinutesId").hide();
    }
}

function PreRenderBySyncType() {
    var e = document.getElementById("syncTypeId");
    if (e.options[e.selectedIndex].text === "Delta") {
        $("#custDeltaStart").show();
        $("#syncScheduleSubtractFromMinutesId").show();
    } else {
        $("#custDeltaStart").hide();
        $("#syncScheduleSubtractFromMinutesId").hide();
    }
}

function SetTargetControl() {
    var selectedTarget = $("#targetType option:selected").text();

    $.ajax({
        type: "Get",
        url: "~/../../api/SnowApi/GetSyncTargetsByType",
        dataType: "json",
        contentType: "application/json; charset=utf8",
        data: { targetType: selectedTarget, SyncId: $("#synchronizationId").val()},
        success: function (res) {
            if (res) {
                var targetList = res.TargetList;
                var targetName = "";
                for (var i = 0; i < targetList.length; i++) {
                    targetName += '<option value="' + targetList[i].Targetname + '">' + targetList[i].Targetname + '</option>';
                }
                $("#targetId").html(targetName);

                //finally set selected target
                if (res.SelectedTargetName !== "" || res.SelectedTargetName != undefined) {
                    var element = document.getElementById("targetId");
                    element.value = res.SelectedTargetName;
                }
            }
        },
        error: function (xhr, status, error) {
            var err = eval("(" + xhr.responseText + ")");
            alertify.error(err);
        }
    });

    if (selectedTarget === "Sql") {
        //target is SqlDb
        $("#selectedDbRow").show();
        $("#schedulerKafkaBlockSizeId").hide();
        $("#schedulerKafkaModeId").hide();
    } else if (selectedTarget === "Kafka") {
        //target is Kafka
        $("#selectedDbRow").hide();
        $("#schedulerKafkaBlockSizeId").show();
        $("#schedulerKafkaModeId").show();
    }
}

function SaveCollectedSyncData() {
    var syncScheduler = new Object();

    var syncId = $("#synchronizationId").val();
    var syncName = $("#synchronizationName").val();
    var syncType = $("#syncTypeId").val();
    var target = $("#targetId").val();
    var databaseSettings = $("#databaseSettingsId").val();
    var instanzSettings = $("#instanzSettingsId").val();
    var threadsPerTable = $("#syncScheduleThreadsPerTable").val();
    var threadSleepTime = $("#syncScheduleThreadSleepTime").val();
    var requestTimout = $("#syncScheduleRequestTimeout").val();
    var pageSize = $("#syncSchedulePageSize").val();
    var kafkaBlockSize = $("#syncScheduleKafkaBlockSize").val();
    var kafkaMode = $("#selectedKafkaModeId").val();
    var syncInterval = $("#syncIntervalId").val();
    var activeSince = $("#syncScheduleDatePicker").val();
    var syncTime = $("#syncScheduleTimePicker").val();
    var intervalInMinutes = $("#syncScheduleIntervalInMinutes").val();

    //custom delta start
    var custDeltaStartDatepicker = $("#custDeltaStartPicker").data("kendoDateTimePicker");
    var custDeltaStart = custDeltaStartDatepicker.value();
    var subtractMinutesFromDelta = $("#syncScheduleSubtractFromMinutes").val();
    
    var selectedDaysOfWeek = [];

    if ($("#chkSunday")[0].checked === true) {
        var dayOfWeekSunday = new Object();
        dayOfWeekSunday.day = "Sunday";
        dayOfWeekSunday.id = 0;
        selectedDaysOfWeek.push(dayOfWeekSunday);
    }
    if ($("#chkMonday")[0].checked === true) {
        var dayOfWeekMonday = new Object();
        dayOfWeekMonday.day = "Monday";
        dayOfWeekMonday.id = 1;
        selectedDaysOfWeek.push(dayOfWeekMonday);
    }
    if ($("#chkTuesday")[0].checked === true) {
        var dayOfWeekTuesday = new Object();
        dayOfWeekTuesday.day = "Tuesday";
        dayOfWeekTuesday.id = 2;
        selectedDaysOfWeek.push(dayOfWeekTuesday);
    }
    if ($("#chkWednesday")[0].checked=== true) {
        var dayOfWeekWednesday = new Object();
        dayOfWeekWednesday.day = "Wednesday";
        dayOfWeekWednesday.id = 3;
        selectedDaysOfWeek.push(dayOfWeekWednesday);
    }
    if ($("#chkThursday")[0].checked === true) {
        var dayOfWeekThursday = new Object();
        dayOfWeekThursday.day = "Thursday";
        dayOfWeekThursday.id = 4;
        selectedDaysOfWeek.push(dayOfWeekThursday);
    }
    if ($("#chkFriday")[0].checked === true) {
        var dayOfWeekFriday = new Object();
        dayOfWeekFriday.day = "Friday";
        dayOfWeekFriday.id = 5;
        selectedDaysOfWeek.push(dayOfWeekFriday);
    }
    if ($("#chkSaturday")[0].checked === true) {
        var dayOfWeekSaturday = new Object();
        dayOfWeekSaturday.day = "Saturday";
        dayOfWeekSaturday.id = 6;
        selectedDaysOfWeek.push(dayOfWeekSaturday);
    }
    
    syncScheduler.SynchronizationId = syncId;
    syncScheduler.SynchronizationName = syncName;
    syncScheduler.ActiveSince = activeSince;
    syncScheduler.Time = syncTime;
    syncScheduler.SelectedTarget = target;
    syncScheduler.SelectedSyncType = syncType;
    syncScheduler.SelectedDatabaseSettings = databaseSettings;
    syncScheduler.SelectedInstanzSettings = instanzSettings;
    syncScheduler.SelectedInterval = syncInterval;
    syncScheduler.IntervalInMinutes = intervalInMinutes;
    syncScheduler.SelectedDaysOfWeek = selectedDaysOfWeek;
    syncScheduler.ThreadsPerTable = threadsPerTable;
    syncScheduler.ThreadSleepTime = threadSleepTime;
    syncScheduler.RequestTimeout = requestTimout;
    syncScheduler.PageSize = pageSize;
    syncScheduler.KafkaBlockSize = kafkaBlockSize;
    syncScheduler.KafkaMode = kafkaMode;
    syncScheduler.CustomDeltaStart = custDeltaStart;
    syncScheduler.SubtractMinutesFromDelta = subtractMinutesFromDelta;
    
    $.ajax({
        type: "POST",
        url: "~/../../api/SnowApi/SetFinishedSchedulerParams",
        dataType: "json",
        contentType: "application/json; charset=utf8",
        data: JSON.stringify(syncScheduler),
        success: function (res) {
            if (res.Success) {

                window.location.href = res.RedirectUrl;
            }
        },
        error: function (xhr, status, error) {
            var err = eval("(" + xhr.responseText + ")");
            alertify.error(err);
        }
    });
}