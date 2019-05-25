function spAddProduct(docToCreate) {
    if (docToCreate.shipping === undefined || docToCreate.shipping.dimensions === undefined || docToCreate.shipping.dimensions.width === undefined) {
        throw new Error('Expected document to contain docToCreate.shipping.dimensions.width.');
    }

    var context = getContext();
    var collection = context.getCollection();
    var response = context.getResponse();

    collection.createDocument(collection.getSelfLink(), docToCreate, {},
        function (err, docCreated) {
            if (err) {
                throw new Error('Error creating document: ' + err.Message);
            }
            response.setBody(docCreated);
        }
    );
}