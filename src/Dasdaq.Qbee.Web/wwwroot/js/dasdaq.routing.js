LazyRouting.SetRoute({
    '/home': null,
    '/currency/all': null,
    '/currency/:id': { id: '[A-Z]{1,10}' },
    '/ite': null,
    '/eosio': null,
    '/exchange': null
});

LazyRouting.SetMirror({
    '/': '/home',
    '/currency': '/currency/all'
});