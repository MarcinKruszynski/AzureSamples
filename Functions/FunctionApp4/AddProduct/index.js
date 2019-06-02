module.exports = async function (context, req) {

    context.log('JavaScript HTTP trigger function processed a request.');

    var product = context.bindings.product

    if (product) {
        context.res = {
            status: 422,
            body: "Product already exists.",
            headers: {
                'Content-Type': 'application/json'
            }
        }
    }    
    else {

        var productString = JSON.stringify({
            id: req.body.id,
            name: req.body.name,
            price: req.body.price,
            stockQuantity: req.body.stockQuantity
        });

        context.bindings.newProduct = productString;

        context.res = {
            status: 200,
            body: "product added!",
            headers: {
                'Content-Type': 'application/json'
            }
        };
    }  
      
};