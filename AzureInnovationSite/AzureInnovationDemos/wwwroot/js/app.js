var app = new Vue({
    el: '#app',
    data: {
        demos: [],
        user: {}
    },
    mounted: function () {
        this.getDemoUser();
    },
    updated: function () {
        setFooterPosition();
    },
    methods: {
        getDemos: function () {
            axios.get('/api/demos')
                .then(response => {
                    this.demos = response.data;
                });
        },
        getDemoUser: function () {
            axios.get('/api/demos/user')
                .then(response => {
                    this.user = response.data;
                    this.getDemos();
                });
        },
        goToDemo: function (demoId, isDisabled) {
            if (!isDisabled) {

                ga('send', 'event', {
                    'eventCategory': 'click',
                    'eventAction': 'goToDemo',
                    'eventLabel':'demo',
                    'eventValue': demoId
                });
                window.location = 'demo/' + demoId;
            }
        }
    }
});




