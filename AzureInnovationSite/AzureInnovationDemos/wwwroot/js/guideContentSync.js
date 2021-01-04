var guideContent = new Vue({
    el: '#guidContentUpdate',
    data: {
        syncingContent: false
    },
    mounted: function () {
        initHubConnection();
    },
    updated: function () {

    },
    methods: {
        syncMDContent: function () {
            let self = this;
            self.syncingContent = true;
            axios.post('/api/admin/syncmdcontent')
                .then(response => {
                    self.syncingContent = false;
                },
                    error => {
                        self.syncingContent = false;
                        $('.alert-danger').show();
                    });
        }
    }
});

$(window).resize(
    function () {
        setFooterPosition();
        $("#guidContentUpdate").css('width', $("article").css('width'));
    });