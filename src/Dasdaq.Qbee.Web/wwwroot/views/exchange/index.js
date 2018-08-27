component.data = function () {
    return {
        total: 100000,
        asset: "10.0000 POM",
        balances: [],
        buys: [],
        sells: []
    };
};

component.created = function () {
    setInterval(() => {
        try {
            this.getBalance();
        } catch (ex) {
        }
        try {
            this.getTrades();
        } catch (ex) {
        }
    }, 5000);
};

component.methods = {
    getTrades: function () {
        var self = this;
        app.eos.getTableRows({
            code: 'pomelo',
            scope: 'pomelo',
            table: 'sellrecord',
            json: true,
        }).then(data => {
            self.sells = data.rows;
        });

        app.eos.getTableRows({
            code: 'pomelo',
            scope: 'pomelo',
            table: 'buyrecord',
            json: true,
        }).then(data => {
            self.buys = data.rows;
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