// For an introduction to the Blank template, see the following documentation:
// http://go.microsoft.com/fwlink/?LinkId=232509
(function () {
    "use strict";

    WinJS.Binding.optimizeBindingReferences = true;

    var app = WinJS.Application;
    var activation = Windows.ApplicationModel.Activation;

    app.onactivated = function (args) {
        if (args.detail.kind === activation.ActivationKind.launch) {
            showProgress();
            if (args.detail.previousExecutionState !== activation.ApplicationExecutionState.terminated) {
                // TODO: This application has been newly launched. Initialize
                // your application here.

                //Handle start push
                PushSDK.NotificationService.handleStartPush(args.detail.arguments);


            } else {
                // TODO: This application has been reactivated from suspension.
                // Restore application state here.
            }
            args.setPromise(WinJS.UI.processAll());

        }
        hideProgress();
    };

    app.oncheckpoint = function (args) {
        // TODO: This application is about to be suspended. Save any state
        // that needs to persist across suspensions here. You might use the
        // WinJS.Application.sessionState object, which is automatically
        // saved and restored across suspension. If you need to complete an
        // asynchronous operation before your application is suspended, call
        // args.setPromise().

    };

    app.start();

})();

function subscribeClick(mouseEvent) {
    try {

        var host = document.getElementById('host').value;
        var id = document.getElementById('PwAppId').value;
        var service = new PushSDK.NotificationService.getCurrent(id);
        service.setHost(host);

        service.ononpushaccepted = function (args) {
            var md = new Windows.UI.Popups.MessageDialog(args.toString());
            md.showAsync()
        }

        service.subscribeToPushService();

        var fieldNameElement = document.getElementById('userToken');
        if (fieldNameElement.firstChild && service.pushToken != null) {
            fieldNameElement.firstChild.nodeValue = service.pushToken;
        }

        service.ononpushtokenreceived = function (pushToken) {
            var fieldNameElement = document.getElementById('userToken');
            if (fieldNameElement.firstChild) {
                fieldNameElement.firstChild.nodeValue = pushToken;
            }
        }

        service.ononpushtokenfailed = function (error) {
            var fieldNameElement = document.getElementById('userToken');
            if (fieldNameElement.firstChild) {
                fieldNameElement.firstChild.nodeValue = error;
            }
        }
    }
    catch (ex_var) {
        txt = "Subscribe failed. Check subscribe parameters and retry: \n" + ex_var.message + "\n";
        console.log(txt);

        var md = new Windows.UI.Popups.MessageDialog(txt);
        md.showAsync()
    }
}

var showProgress = function () {
    var progress = document.getElementById('p');
    progress.style.visibility = 'visible';
};
var hideProgress = function () {
    var progress = document.getElementById('p');
    progress.style.visibility = 'hidden';
};

function unsubscribeClick(mouseEvent) {
    try {
        var service = new PushSDK.NotificationService.getCurrent(null);
        if(service != null)
            service.unsubscribeFromPushes(null, null);

        var fieldNameElement = document.getElementById('userToken');
        if (fieldNameElement.firstChild)
            fieldNameElement.firstChild.nodeValue = "wait token...";
    }
    catch (ex_var) {
        txt = "Unsubscribe failed: \n" + ex_var.message + "\n";
        console.log(txt);

        var md = new Windows.UI.Popups.MessageDialog(txt);
        md.showAsync()
    }
}


function setGeozone() {

    var service = new PushSDK.NotificationService.getCurrent(null);

    var check = document.getElementById('geozone');

    if (service != null) {
        if (service.pushToken != null) {

            if (check.checked) {
                try {
                    service.startGeoLocation();
                    txt = "Geozones enabled";
                    console.log(txt);0

                    var md = new Windows.UI.Popups.MessageDialog(txt);
                    md.showAsync()
                }
                catch (ex_var) {
                    txt = "Geolocation start errror: \n" + ex_var.message + "\n";
                    console.log(txt);

                    var md = new Windows.UI.Popups.MessageDialog(txt);
                    md.showAsync()
                }
            }
            else {
                try {
                    service.stopGeoLocation();
                    txt = "Geozones disabled";
                    console.log(txt);

                    var md = new Windows.UI.Popups.MessageDialog(txt);
                    md.showAsync()
                }
                catch (ex_var) {
                    txt = "Geolocation stop error: \n" + ex_var.message + "\n";
                    console.log(txt);

                    var md = new Windows.UI.Popups.MessageDialog(txt);
                    md.showAsync()
                }
            }
        }
    }
}

function sendingTagClick(mouseEvent) {

    var tagValue = document.getElementById('tagValue').value;
    var _key = document.getElementById('tagTitle').value;
    var _value;

    try {

        if (tagValue.indexOf(',') != -1)
            _value = tagValue.Replace(", ", ",").Split(',');
        else
            _value = tagValue;

    }
    catch (exception_var) {
        txt = "Sending the tags exception: \n" + exception_var.message + "\n";
        console.log(txt);
    }

    var keys = [_key];
    var values = [_value];

    var service = new PushSDK.NotificationService.getCurrent(null);
    if (service != null) {
        if (service.pushToken != null) {
            try {
                var tag = new PushSDK.NotificationService.getCurrent(null);

                tag.getTags(
                    function (caller, tags) {
                        var md = new Windows.UI.Popups.MessageDialog(tags);
                        md.showAsync();
                    }, function (error) {
                        var md = new Windows.UI.Popups.MessageDialog("failed to get tags");
                        md.showAsync()
                    });

                tag.sendTag(keys, values, null, null);
                txt = "Tag has been sent!";
                console.log(txt);

            }
            catch (ex_var) {
                txt = "Sending the tags exception: \n" + ex_var.message + "\n";

                console.log(txt);

                var md = new Windows.UI.Popups.MessageDialog(txt);
                md.showAsync()
            }
        }
    }
}
