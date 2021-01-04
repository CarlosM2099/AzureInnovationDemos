$(window).resize(setFooterPosition);

$('body').ready(function () {
    setFooterPosition();
    $('div.close').on("click", "", function () {       
        $('.alert-danger').hide();
    });
});

function setFooterPosition() {
    var body = $('body')[0];

    if (body.scrollHeight > body.clientHeight) {
        $('footer').css('position', '');
    }
    else {
        $('footer').css('position', 'absolute');
    }
}


function formatLocateUTCDate(dateStr) {
    if (dateStr) {
        var date = new Date(dateStr.replace('T', ' ') + " UTC");
        var hours = date.getHours();
        var minutes = date.getMinutes();
        var ampm = hours >= 12 ? 'pm' : 'am';
        hours = hours % 12;
        hours = hours ? hours : 12; // the hour '0' should be '12'
        minutes = minutes < 10 ? '0' + minutes : minutes;
        var strTime = hours + ':' + minutes + ' ' + ampm;
        return date.getMonth() + 1 + "/" + date.getDate() + "/" + date.getFullYear() + "  " + strTime;
    }
}
 