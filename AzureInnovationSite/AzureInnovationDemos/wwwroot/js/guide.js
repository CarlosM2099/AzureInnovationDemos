var guide = new Vue({
    el: '#guide',
    data: {
        user: {},
        environment: null,
        vm: null,
        assets: [],
        displayResources: false
    },
    mounted: function () {
        if (typeof (environment) !== 'undefined') {
            this.displayResources = true;
            this.environment = environment;

            if (vm) {
                this.vm = vm;
            }
            if (assets) {
                this.assets = assets;
            }
        }
    },
    updated: function () {
        window.setTimeout(function () {
            initGuideAccordion();
        }, 0);
    },
    methods: {
        toggleCode: function (codePanel) {
            var panel = document.getElementsByClassName(codePanel)[0];
            var height = panel.style.maxHeight;
            panel.style.maxHeight = height == 'none' ? '30px' : 'none';
        }
    }
});

new ClipboardJS('.copy-text');

$(window).resize(
    function () {
        setFooterPosition();
        $("#guide").css('width', $("article").css('width'));
    });


