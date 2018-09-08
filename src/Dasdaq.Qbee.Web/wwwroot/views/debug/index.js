﻿component.data = function () {
    return {
        id: null,
        chart: null,
        active: 'kchart',
        total: "0.0000",
        asset: "0.0000"
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
        qv.get('http://dasdaq-webapi.chinacloudsites.cn/api/Candlestick/OneBox-' + this.currency.id, {})
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
    getPublicKey: function () {
        return qv.get(`/api/chain/account/${app.account.name}/perm/${app.account.authority}`, {}).then(x => {
            return Promise.resolve(x.data);
        });
    },
    buy: function () {
        var self = this;
        self.auth()
            .then(() => {
                app.notification('pending', '正在调用buy合约');
                var requiredFields = app.requiredFields;
                app.eos.contract('pomelo', { requiredFields }).then(contract => {
                    return contract.buy(app.account.name, self.asset + " " + self.id, parseInt(self.total * 10000), { authorization: [`${app.account.name}@${app.account.authority}`] });
                })
                    .then(() => {
                        app.notification('succeeded', 'buy合约调用成功');
                    })
                    .catch((err) => {
                        app.notification('error', 'buy合约调用失败', err.toString());
                    });
            });
    },
    sell: function () {
        var self = this;
        self.auth()
            .then(() => {
                app.notification('pending', '正在调用sell合约');
                var requiredFields = app.requiredFields;
                app.eos.contract('pomelo', { requiredFields }).then(contract => {
                    return contract.sell(app.account.name, self.asset + " " + self.id, parseInt(self.total * 10000), { authorization: [`${app.account.name}@${app.account.authority}`] });
                })
                    .then(() => {
                        app.notification('succeeded', 'sell合约调用成功');
                    })
                    .catch((err) => {
                        app.notification('error', 'sell合约调用失败', err.toString());
                    });
            });
    },
    auth: function () {
        app.notification('pending', '正在对合约账户授权');
        return this.getPublicKey()
            .then(key => {
                return app.eos.updateauth({
                    account: app.account.name,
                    permission: app.account.authority,
                    parent: "owner",
                    auth: {
                        "threshold": 1,
                        "keys": [{
                            "key": key,
                            "weight": 1
                        }],
                        "accounts": [{
                            "permission": {
                                "actor": "pomelo",
                                "permission": "eosio.code"
                            },
                            "weight": 1
                        }]
                    }
                });
            })
            .then(() => {
                app.notification('succeeded', '对合约账户授权成功');
                return Promise.resolve(null);
            })
            .catch(err => {
                app.notification('error', '对合约账户授权失败', err.toString());
                return Promise.reject(err);
            });
    }
};

component.computed = {
    currency: function () {
        var self = this;
        return app.currencies.filter(x => x.id === self.id)[0];
    }
};