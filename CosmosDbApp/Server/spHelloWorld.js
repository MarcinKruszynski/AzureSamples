function spHelloWorld() {
    var context = getContext();
    var response = context.getResponse();
    response.setBody('Hello Cosmos DB!');
}