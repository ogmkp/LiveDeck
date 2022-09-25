const express = require('express')
const bodyParser = require('body-parser')
const cors = require('cors')
const { MongoClient, ServerApiVersion} = require('mongodb');
const app = express()
const port = 8080

app.use(cors());

app.use(bodyParser.urlencoded({ extended: false }));
app.use(bodyParser.json());

const uri = "mongodb://127.0.0.1:27017"
const client = new MongoClient(uri, { useNewUrlParser: true, useUnifiedTopology: true, serverApi: ServerApiVersion.v1 });

client.connect((err, prom) => {
    if (err) return console.error(err)
    console.log('Connected to DB')
})

app.delete('/api/user/:utoken', async (req,res) => {
    const utoken = req.params.utoken;

    const query = {"UToken": utoken};

    await client
        .db("livedeck")
        .collection("accounts")
        .deleteOne(query);

    await client
        .db("livedeck")
        .collection("panel")
        .deleteOne(query, (err, results) => {
            if (err) {
                res.json({'status': 'error'});
                return
            }
            res.json({'status': 'changed'});
        });
})

app.get('/api/admin/validate', async (req,res) => {
    let adminToken = req.query.AdminToken;

    if (adminToken === undefined) {
        res.status(500).send('AdminToken not specified')
        return
    }

    const query = { 'Username': 'admin' };

    try {
        await client
            .db("livedeck")
            .collection('accounts')
            .findOne(query)
            .then(async doc => {
                if (doc === null) {
                    res.status(404).send('Document not found')
                    return
                }

                let token = doc.AdminToken;

                res.json({Valid: token === adminToken});
            });
    } catch (err) {
        console.error(err);
        res.status(500).send('Server error')
        return
    }
})


app.post('/api/user/updateMail', async (req,res) => {

})

app.post('/api/user/changeUToken', async (req,res) => {

})

app.post('/api/user/changePassword', async (req,res) => {

})

app.get('/api/info/getallinfo', async (req,res) => {
    let adminToken = req.query.AdminToken;

    if (adminToken === undefined) {
        res.status(500).send('Admintoken not specified')
        return
    }
    const query = { 'Username': 'admin' };

    try {
        await client
            .db("livedeck")
            .collection('accounts')
            .findOne(query)
            .then(async doc => {
                if (doc === null) {
                    res.status(404).send('Document not found')
                    return
                }
                let token = doc.AdminToken;
                if (!(token === adminToken)) {
                    res.status(401).send('Token is wrong')
                    return
                }
                const data = await client
                    .db("livedeck")
                    .collection('accounts')
                    .find().toArray();
                res.json(data);
            });
    } catch (err) {
        console.error(err);
        res.status(500).send('Server error')
        return
    }
})

app.post('/api/controller',async (req,res) => {
    let token = req.query.utoken;

    if (token === undefined) {
        res.status(500).send('UToken not specified')
        return
    }

    if (req.body.propId === undefined
        || req.body.propState === undefined) {
        res.status(500).send('Body is undefined')
        return
    }

    const propId = req.body.propId;
    const propState = req.body.propState;

    const query = { 'UToken': token };
    const new_values = { $set: { [`Index.${propId}`]: Boolean(propState)} }

    await client
        .db("livedeck")
        .collection('panel')
        .updateOne(query, new_values, (err, doc) => {
        if (err) throw err;

        res.status(200);
        res.append('Content-Type', 'application/json');
        res.json({'status': 'changed'});
    });
});

app.get('/api/controller', async (req, res) => {
    let token = req.query.utoken;

    if (token === undefined) {
        res.status(500).send('UToken not specified')
        return
    }
    const query = { 'UToken': token };

    try {
        await client
            .db("livedeck")
            .collection('panel')
            .findOne(query)
            .then(doc => {
                if (doc === null) {
                    res.status(404).send('Not found')
                    return
                }

                res.status(200);
                res.append('Content-Type', 'application/json');
                res.json({ DataState: doc.Index});
            });
        return
    } catch (err) {
        console.error(err);
    }
    res.status(404).send('Not found')
});

app.listen(port, () => {
    console.log(`app listening on port ${port}`)
});