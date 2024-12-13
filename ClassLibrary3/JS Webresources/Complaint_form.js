function SetComplaintDateAsToday() {
    var istoday = Xrm.Page.getAttribute('rr_today').getValue();

    if (istoday == true) {
        var currentDate = new Date();
        Xrm.Page.getAttribute('rr_complaintdate').setValue(currentDate);
    }
    else {
        Xrm.Page.getAttribute('rr_complaintdate').setValue(null);
    }
}