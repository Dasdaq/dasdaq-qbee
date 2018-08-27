component.data = function () {
    return {
        id: null,
        chart: null,
        active: 'kchart',
        balances: []
    };
};

component.created = function () {
    app.active = 'currency';
    var self = this;
    self.id = router.history.current.params.id;
    setTimeout(function () {
        self.chart = echarts.init(document.getElementById('kchart'));
        self.chart.setTheme('dark');
        self.renderChart();
    }, 1000);
};

component.methods = {
    renderChart: function () {
        var self = this;
        qv.get('http://dasdaq-webapi.chinacloudsites.cn/api/Candlestick/OneBox-'/* + this.currency.id*/, {})
            .then(data => {
                var x = data.data.values.map(y => y.time);
                var y = data.data.values.map(z => [z.opening, z.closing, z.min, z.max]);
                var option = {
                    title: {
                        text: ''
                    },
                    tooltip: {
                        trigger: 'axis',
                        formatter: function (params) {
                            var res = params[0].seriesName + ' ' + params[0].name;
                            res += '<br/>  开盘 : ' + params[0].value[0] + '  最高 : ' + params[0].value[3];
                            res += '<br/>  收盘 : ' + params[0].value[1] + '  最低 : ' + params[0].value[2];
                            return res;
                        }
                    },
                    legend: {
                        data: [self.id]
                    },
                    toolbox: {
                        show: false
                    },
                    dataZoom: {
                        show: true,
                        realtime: true,
                        start: 50,
                        end: 100
                    },
                    xAxis: [
                        {
                            type: 'category',
                            boundaryGap: true,
                            axisTick: { onGap: false },
                            splitLine: { show: false },
                            data: x
                        }
                    ],
                    yAxis: [
                        {
                            type: 'value',
                            scale: true,
                            boundaryGap: [0.01, 0.01]
                        }
                    ],
                    series: [
                        {
                            name: self.id,
                            type: 'k',
                            data: y
                        }
                    ]
                };
                console.warn(option);
                self.chart.setOption(option);
            });
    },
    getBalance: function () {
        if (app.account.name) {
            var self = this;
            app.eos.getCurrencyBalance('eosio.token', app.account.name).then(x => {
                self.balances = x;
            });
        }
    },
};

component.computed = {
    currency: function () {
        var self = this;
        return app.currencies.filter(x => x.id === self.id)[0];
    }
};