var demo = new Vue({
    el: '#demo',
    data: {
        demo: {},
        user: {},
        generatingDemo: false,
        demoId: 0,
        provisioningDemoStatus: '',
        availableDemos: false,
        availableDemosCount: 0,
        nextAvailableDemoEnv: '',
        demoExpirationDate: '',
        validatingDemoResources: false
    },
    mounted: function () {

        this.getDemoUser();
    },
    updated: function () {
        setFooterPosition();
    },
    methods: {
        getDemo: function () {
            this.demoId = window.location.pathname.replace('/demo/', '');

            axios.get('/api/demos/' + this.demoId)
                .then(response => {
                    this.demo = response.data;
                    this.getDemoUserEnvironments();
                }, error => {

                    $('.alert-danger').show();
                });
        },
        createEnvironment: function (demoId) {
            let self = this;
            self.generatingDemo = true;
            self.provisioningDemoStatus = 'Creating demo user';

            ga('send', 'event', {
                'eventCategory': 'click',
                'eventAction': 'createEnvironment',
                'eventLabel': 'demo-environment',
                'eventValue': demoId
            });

            axios.post('/api/demos/' + demoId + '/provisionenvuser')
                .then(response => {

                    self.provisioningDemoStatus = 'Generating demo environment';
                    let provisionedUser = response.data;
                    if (this.demo.type.name === 'ADODemo') {
                        axios.post('/api/demos/' + demoId + '/provisionorg', provisionedUser).then(response => {

                            self.provisioningDemoStatus = 'Configuring demo environment';

                            axios.post('/api/demos/' + demoId + '/provisionenv/' + response.data.name, provisionedUser)
                                .then(response => {

                                    self.generatingDemo = false;
                                    self.getDemoUserEnvironments();
                                },
                                    error => {
                                        self.generatingDemo = false;
                                        $('.alert-danger').show();
                                    });

                        }, error => {
                            self.generatingDemo = false;
                            $('.alert-danger').show();
                        });
                    }
                    else {
                        self.provisioningDemoStatus = 'Configuring demo environment';

                        axios.post('/api/demos/' + demoId + '/provisionenv/NonOrg', provisionedUser)
                            .then(response => {

                                self.generatingDemo = false;
                                self.getDemoUserEnvironments();
                            },
                                error => {
                                    self.generatingDemo = false;
                                    $('.alert-danger').show();
                                });
                    }
                },
                    error => {
                        self.generatingDemo = false;
                        $('.alert-danger').show();
                    });
        },
        getDemoUser: function () {
            console.log(window.location.href);
            axios.get('/api/demos/user')
                .then(response => {
                    this.user = response.data;
                    this.getDemo();
                }, error => {
                    $('.alert-danger').show();
                });
        },
        getDemoUserEnvironments: function () {
            let self = this;
            axios.get('/api/demos/' + this.demoId + '/user/' + this.user.id + '/environments')
                .then(response => {
                    response.data.map(function (value, key) {

                        if (self.demo.id === value.demoId) {
                            self.demo.abstract = self.demo.abstract + " ";
                            self.demo.environment = value;
                            if (value.environmentProvisioned) {
                                self.getDemoVM(self.demo.id);
                                self.getDemoUserResources(self.demo.id);

                                self.getDemoExpiration();
                            }
                            else {
                                window.setTimeout(self.getDemoUserEnvironments, 30000);
                            }
                        }
                        else {
                            self.getDemoAvailability();
                        }
                    });

                    if (response.data.length === 0) {
                        self.getDemoAvailability();
                    }
                },
                    error => {
                        $('.alert-danger').show();
                    });
        },
        getDemoVM: function (demoId) {
            axios.get('/api/demos/' + demoId + '/vm')
                .then(response => {

                    if (response.data && this.demo.id === response.data.demoId) {
                        this.demo.abstract = this.demo.abstract + " ";
                        this.demo.vm = response.data;
                    }

                },
                    error => {
                        $('.alert-danger').show();
                    });
        },
        getDemoUserResources: function (demoId) {
            axios.get('/api/demos/' + demoId + '/user/' + this.user.id + '/resources')
                .then(response => {

                    if (response.data) {
                        this.demo.assets = response.data.concat(this.demo.assets);
                    }

                },
                    error => {
                        $('.alert-danger').show();
                    });
        },
        getDemoExpiration: function () {

            axios.get('/api/demos/' + this.demoId + '/user/' + this.user.id + '/resourceexpiration')
                .then(response => {

                    if (response.data) {

                        this.demoExpirationDate = formatLocateUTCDate(response.data.expirationDate);
                    }

                },
                    error => {
                        $('.alert-danger').show();
                    });
        },
        getDemoAvailability: function () {
            if (!this.demo.isSharedEnvironment) {
                this.validatingDemoResources = true;
                axios.get('/api/demos/' + this.demoId + '/validateresources')
                    .then(response => {

                        if (response.data) {
                            this.availableDemos = response.data.availableResources;
                            this.availableDemosCount = response.data.availableResourcesCount;
                            if (!response.data.availableResources) {
                                this.nextAvailableDemoEnv = formatLocateUTCDate(response.data.nextAvailable);
                            }
                            this.validatingDemoResources = false;
                        }

                    },
                        error => {
                            $('.alert-danger').show();
                        });
            }
        },
        gotAsset: function (asset) {
            ga('send', 'event', {
                'eventCategory': 'click',
                'eventAction': 'gotAsset',
                'eventLabel': 'demo-asset',
                'eventValue': asset.id
            });
        },
        toggleCode: function (codePanel) {
            var panel = document.getElementsByClassName(codePanel)[0];
            var height = panel.style.maxHeight;
            panel.style.maxHeight = height == 'none' ? '30px' : 'none';
        }
    }
});

new ClipboardJS('.copy-text');

