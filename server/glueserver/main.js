"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var ytdl = require("ytdl-core");
var path = require("path");
var express = require("express");
var crypto_1 = require("crypto");
var node_datachannel_1 = require("node-datachannel");
var bodyParser = require("body-parser");
var cookieParser = require("cookie-parser");
// Creating express app
var app = express();
// Serving static files in the public folder
app.use('/', express.static(path.join(__dirname, '/public')));
app.use(bodyParser.json());
app.use(cookieParser());
var peers = new Map();
app.post('/sdp', function (req, res) {
    var _a;
    if (req.body.requestSdp) {
        var id_1 = (0, crypto_1.randomUUID)();
        var con_1 = new node_datachannel_1.PeerConnection('Server', { iceServers: ['stun:stun.l.google.com:19302'] });
        con_1.onStateChange(function (e) {
            console.log('state change', id_1, e);
        });
        var candidates_1 = [];
        con_1.onLocalCandidate(function (candidate, mid) {
            candidates_1.push({ c: candidate, m: mid });
        });
        con_1.onGatheringStateChange(function (s) {
            if (s === 'complete') {
                con_1.setLocalDescription("offer" /* DescriptionType.Offer */);
                res.cookie('session', id_1).send(JSON.stringify({
                    desc: {
                        sdp: con_1.localDescription().sdp,
                        type: "offer" /* DescriptionType.Offer */
                    },
                    candidates: candidates_1
                }));
            }
        });
        var udp = con_1.createDataChannel('UDP', { ordered: true, maxRetransmits: 0 });
        var tcp = con_1.createDataChannel('TCP');
        peers.set(id_1, { con: con_1, udp: udp, tcp: tcp });
    }
    else {
        if (!((_a = req.cookies) === null || _a === void 0 ? void 0 : _a.session)) {
            res.status(500).send();
            return;
        }
        var id = req.cookies.session;
        var peer = peers.get(id);
        peer.con.setRemoteDescription(req.body.desc.sdp, req.body.desc.type);
        for (var _i = 0, _b = req.body.candidates; _i < _b.length; _i++) {
            var candidate = _b[_i];
            peer.con.addRemoteCandidate(candidate.c, candidate.m);
        }
        res.send();
    }
});
app.get('/yt', function (req, res) {
    var _a;
    if (!((_a = req.query) === null || _a === void 0 ? void 0 : _a.url)) {
        res.status(406).send();
        return;
    }
    ytdl.getInfo(req.query.url, { requestOptions: {} })
        .then(function (info) {
        var _a, _b;
        res.send((_b = (_a = info.formats.filter(function (f) { return f.hasAudio && f.hasVideo; })) === null || _a === void 0 ? void 0 : _a[0]) === null || _b === void 0 ? void 0 : _b.url);
    })
        .catch(function () { return res.status(406).send(); });
});
var port = 80;
app.listen(port, function () {
    console.log("server listening at http://localhost:".concat(port));
});
