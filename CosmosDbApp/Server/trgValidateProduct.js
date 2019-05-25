function trgValidateProduct() {

    var context = getContext();
    var request = context.getRequest();
    var doc = request.getBody();

    if (doc.shipping === undefined || doc.shipping.weight === undefined) {
        throw new Error('Expected document to contain shipping.weight');
    }

    if (doc.shipping.weight < 0)
        doc.shipping.weight = 0;

    request.setBody(doc);
}