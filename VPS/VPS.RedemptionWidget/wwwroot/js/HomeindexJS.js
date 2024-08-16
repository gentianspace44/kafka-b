// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
window.dataLayer = window.dataLayer || [];
window.dataLayer.push({
    'event': 'deposit',
    'revenue': 'integer_value' // ^([0-9]+)$
});

window.dataLayer = window.dataLayer || [];
window.dataLayer.push({
    'event': 'all_gateway_deposits'
});



$.fn.StartWidget = function (baseControlCenterApi) {
    const IDK = "#DefaultVoucherProvider";
    const ProviderBtn = ".btnProvider";
    const ProviderInput = "#VoucherName";
    var hasSignalRResponded = false;

    $(ProviderBtn).click(function () {
        
        const closestRadioButton = $(this).find('input[type="radio"]');
        closestRadioButton.prop('checked', true);
        $(ProviderInput).val($(this).find('input[type="radio"]').val());
        $(IDK).prop("checked", false);
        $(ProviderBtn).removeClass("active");
        $(this).addClass("active");
    });

    $(IDK).click(function () {       
        $(ProviderBtn).removeClass("active");
    });

    $(document).on('keypress', ':input[type="number"]', function (e) {
        if (isNaN(e.key)) {
            return false;
        }
    });

    function updateSelectedProvider(provider) {
        $('.btnProvider').removeClass('selected-provider');
        provider.closest('.btnProvider').addClass('selected-provider');
        provider.closest('.btnProvider').find('.svg-color').show();
        $('.btnProvider').not(provider.closest('.btnProvider')).find('.svg-color').hide();
        $('.btnProvider').not(provider.closest('.btnProvider')).css('border-color', '');
    }
    

    $('input[name="selectedProvider"]').click(function () {
        let selectedProvider = $(this);
        let closestRadioButton = selectedProvider.closest('input[type="radio"]');
        closestRadioButton.prop('checked', true);
        updateSelectedProvider(selectedProvider);
    });

    $('input[name="DefaultVoucherProvider"]').change(function () {
        if ($(this).is(':checked')) {
            $('input[name="selectedProvider"]').prop('checked', false);
            $('.btnProvider').removeClass('selected-provider');
            $('.btnProvider .svg-color').hide();
            $('.btnProvider').css('border-color', '');
        }
    });
    $(document).on('keypress', ':input[type="number"]', function (e) {
        if (isNaN(e.key)) {
            return false;
        }
    });

    $('#btnSubmit').click(function () {
        const selectedProvider = $('input[name="selectedProvider"]:checked');

        // Extract the voucher name from the radio button's value attribute
        const voucherName = selectedProvider.val();

        // Extract the SyxVoucherCreditingEndPoint value from the checked radio button's data attribute
        const voucherEndpoint = selectedProvider.data('syxvouchercreditingendpoint');

        // Extract the voucher length using the data method
        const voucherlengths = selectedProvider.data('voucherlength');

        // Set the selected voucher name and SyxVoucherCreditingEndPoint in the hidden input fields
        $('#VoucherName').val(voucherName);
        $('#SyxVoucherCreditingEndPoint').val(voucherEndpoint);

        // Set the voucher length in the hidden input field
        $('#VoucherNumberLength').val(voucherlengths);

        hasSignalRResponded = false;

        // Submit the form
        AjaxPostForm();
    });

    $('#VoucherNumber').on('input', function () {
        $(this).val($(this).val().replace(/\D/g, ''));
    });

    function AjaxPostForm() {
        const formData = getFormValues(); // Serialize form data
        if (formData.VoucherNumber == undefined || formData.VoucherNumber == null || formData.VoucherNumber == '') {
            showMessage('Voucher Pin cannot be empty.', true, 10000);
            return;
        }
        ShowSpinner();
        $("#submitButton").toggle();
        $('#responseMessage').html('').removeClass('alert-danger').removeClass('btn-primary');
        $.ajax({
            type: "POST",
            url: "Home/RedeemVoucher",
            data: formData,
            success: function (response) {
          
                if (response.errorMessage !== null && response.errorMessage !== undefined && response.errorMessage !== '') {
                    showMessage(response.errorMessage, true, 10000);

                    $('#btnSubmit').prop('disabled', false);
                    HideSpinner();
                    return;
                } 

             
                if (response.useSyxCredit === true && (response.errorMessage == null || response.errorMessage == undefined || response.errorMessage == ''))//If call CyxCredit
                {
                    showMessage(response.message, false, 10000);
                    $('#btnSubmit').prop('disabled', false);
                    HideSpinner();
                    return;
                }
                         
                setTimeout(() => {

                    if (!hasSignalRResponded) //SignalR has not Responded
                    {
                        let validationData = {
                            clientId: formData.ClientId,
                            voucherName: formData.VoucherName,
                            voucherPin: formData.VoucherNumber,
                            defaultVoucherProvider: formData.DefaultVoucherProvider,
                            voucherNumberLength: formData.VoucherNumberLength
                        };
                        
                        $.ajax({
                            type: "POST",
                            contentType: 'application/json',
                            url: baseControlCenterApi + "VerifyRedemptionStatus",
                            data: JSON.stringify(validationData),
                            success: function (response) {                              
                                if (response.isCreditedOnSyx == true && !hasSignalRResponded) {
                                    showMessage(response.message, false, 10000);
                                    $('#btnSubmit').prop('disabled', false);
                                    HideSpinner();
                                    return;
                                } else if (response.isCreditedOnSyx == false && !hasSignalRResponded) {
                                    showMessage(response.message, false, 10000);
                                    $('#btnSubmit').prop('disabled', false);
                                    HideSpinner();
                                    return;
                                }
                            },
                            error: function (error) {
                                showMessage(`An unexpected error has occurred, please try again later.`, false, 3000);

                                $('#btnSubmit').prop('disabled', false);
                                HideSpinner();
                            }
                        });
                    }

                }, 3000);
            },
            error: function (error) {
                showMessage(`An unexpected error has occurred, please try again later.`, false, 3000);

                $('#btnSubmit').prop('disabled', false);
                HideSpinner();
            },
            complete: function () {

            }
        });
    }

    function getFormValues() {
        const formData = {
            ClientId: $("#ClientId").val(),
            DevicePlatform: $("#DevicePlatform").val(),
            HasVoucherProvider: $("#HasVoucherProvider").val(),
            DefaultVoucherProvider: $("#DefaultVoucherProvider").is(":checked"),
            ToggleHelpText: $("#ToggleHelpText").val(),
            VouchersEnablers: $("#VouchersEnablers").val(),
            VoucherName: $("#VoucherName").val(),
            SyxVoucherCreditingEndPoint: $("#SyxVoucherCreditingEndPoint").val(),
            VoucherNumberLength: $("#VoucherNumberLength").val(),
            SignalRConnectionId: $("#SignalRConnectionId").val(),
            VoucherNumber: $("#VoucherNumber").val()
        };

        return formData;
    }


    function showMessage(message, error, timeout) {
        console.log(Date() + " " + message);
        const responseMessage = $('#responseMessage');
        responseMessage.html(message).removeClass(`${!error ? 'alert-danger' : 'btn-primary'}`).addClass(`${error ? 'alert-danger' : 'btn-primary'}`);     
    }

    function TryAndGet() {
        try {
            return $('#ClientId').val();
        }
        catch  {
            return undefined;
        }
    }

    function initializeHubConnection(hubName, url, onEventHandlers, onConnectedHandler) {
        const hubConnection = new signalR.HubConnectionBuilder()
            .withUrl(url)
            .withAutomaticReconnect()
            .build();

        Object.entries(onEventHandlers).forEach(([eventName, handler]) => {
            hubConnection.on(eventName, handler);
        });

        if (onConnectedHandler) {
            hubConnection.on('connected', onConnectedHandler);
        }

        hubConnection.start()
            .then(() => console.log(`${hubName} Hub connection started`))
            .catch((err) => console.error(`Error while starting SignalR connection: ${err}`));

        return hubConnection;
    }

    function handleNotificationsConnected(connectionId) {
        console.log('Connected with connectionId:', connectionId);
        $('#SignalRConnectionId').val(connectionId);

        const clientId = TryAndGet();
        if (clientId !== undefined) {
            notificationsHubConnection.invoke('SetUserId', clientId, connectionId);
        }
    }

    function ShowSpinner() {
        $("#submitButton").css('display', 'none');
        $("#submitButton").empty();
        $("#spinner").css('display', 'inline');  
    }

    function HideSpinner() {
        $("#spinner").css('display', 'none');
        $("#submitButton").css('display', 'inline');
        $("#submitButton").html('Submit');
    }


    const notificationsHubConnection = initializeHubConnection(
        'Notifications',
        baseControlCenterApi + 'notificationsHub',
        {
            'Notify': (message) => {
                hasSignalRResponded = true;
                $('#btnSubmit').prop('disabled', false);
                HideSpinner();
                showMessage(message, false, 3000)
            }
        },
        handleNotificationsConnected
    );

    setTimeout(function () {
        $('#frmVoucher label:nth-child(1)').trigger('click')
    }, 100)
}

