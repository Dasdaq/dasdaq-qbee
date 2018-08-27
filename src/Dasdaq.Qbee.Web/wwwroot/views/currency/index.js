component.data = function () {
    return {
        id: null,
        currency: null
    };
};

component.created = function () {
    app.active = 'currency';
    this.id = router.history.current.params.id;
    this.currency = app.currencies.filter(x => x.id === this.id);
};

component.methods = {
};