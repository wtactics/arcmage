const connection = new signalR.HubConnectionBuilder()
    .withUrl(gameApiUri + "/signalr/games")
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Debug)
    .build();

const scale = 3;

const sizing = {
    battlefield: {
        width: 1920 * scale,
        height: 1200 * scale,
    },
    fieldview: {
        flat: { 
            distancePercentage: 0, 
            rightOffset: (5 + 56 ) * scale,
            leftOffsetPercentage: 0.15
        },
        perspective:{
            distancePercentage: 0.15, 
            rightOffset: 0,
            leftOffsetPercentage: 0.05
        }
    },
    card: {
        width: 106 * scale,
        height: 150 * scale,
    },
    snap: {
        gapX: 30 * scale,
        gapY: 50 * scale
    },
    autoResourceGap: 10 * scale,
    playerDeck: { top: 640 * scale, left: 1774 * scale, width: 106 * scale, height: 150 * scale },
    playerGraveyard: { top: 850 * scale, left: 1774 * scale, width: 106 * scale, height: 150 * scale },
    opponentDeck: { top: 410 * scale, left: 1774 * scale, width: 106 * scale, height: 150 * scale },
    opponentGraveyard: { top: 200 * scale, left: 1774 * scale, width: 106 * scale, height: 150 * scale},
    opponentHand: { top: 0 * scale, left: 800 * scale, width:320 * scale, height: 50 * scale },
    playerHand: { top: 1105 * scale, left: 380 * scale, width: 1160 * scale, height: 95 * scale }
}


const soundIntro = new Howl({
    src: ['audio/soundIntro.ogg', 'audio/soundIntro.mp3'],
    loop: true,
  //  autoplay: true,
    volume: 0.5,
});

const sound = new Howl({
    src: ['audio/sound.ogg', 'audio/sound.mp3'],
    sprite: {
        joinGame: [0, 0],
        // startGame: [27968, 4069],
        startGame: [35912, 4458],
        drawCard: [512, 422],
        discardCard: [26062, 199],
        playCard: [33997, 331],
        deckCard:  [33997, 331],
        removeCard: [0, 0],
        moveCard:[753,211],
        rotateCard:[753,211],
        flipCard:[437,271],
        peekCard: [0, 0],
        pointCard: [0, 0],
        changePlayerStats: [0, 0],
        shuffleList: [6638, 783],
        flipCoin: [17935, 1265],
        diceRoll: [21078, 984], 
        leaveGame: [0, 0],        
    }
  });

/* The vue app, containing the visualisation, and the ui actions */
var vue = new Vue({
    el: '#arcmagegame',
    data: {
        keycode: null,
        gameinfo: [
        {
            title: "Gloabal keyboard shortcuts.",
            items: [
                { key: "Ctrl+Enter, Ctrl+Spacebar", description: "Unmark all your cards." },
                { key: "Shift+Enter, Shift+Spacebare", description: "Unmark all opponents cards (in case you'd like to help out)." },
                { key: "Ctrl+D", description: "Draw one card from your deck." },
                { key: "Ctrl+T", description: "Draw two cards from your deck." },
                { key: "Ctrl+R", description: "Roll a dice." },
                { key: "Ctrl+F", description: "Flip the coin." },
                { key: "Ctrl+B", description: "Toggle the curtain/blinds for your opponent." }
            ]
        },
        {
            title: "Cards operations.",
            items: [
                { key: "Drag and Drop", description: "Move cards around." },
                { key: "Double Click, M+Click, X+Click", description: "Mark or unmark card (rotate)." },
                { key: "Right Click, U+Click", description: "Turn card face down/face up." },
                { key: "L+Click, Shift+Click", description: "Look under card (only when the card is face down)." },
                { key: "P+Click, Ctrl+Click", description: "Point at a card." },
                { key: "G+Click", description: "Put the card in its owner garveyard." },
                { key: "H+Click", description: "Send the card back to its owner hand." },
                { key: "C+Click", description: "Take ownerschip of card (now it will unmark when you unmark all)." },

            ]
        }
        ],
        backgroundImage: 'field11.webp',
        backgrounds: [
          { image: 'field1.webp' },
          { image: 'field2.webp' },
          { image: 'field3.webp' },
          { image: 'field4.webp' },
          { image: 'field5.webp' },
          { image: 'field6.webp' },
          { image: 'field7.webp' },
          { image: 'field8.webp' },
          { image: 'field9.webp' },
          { image: 'field10.webp' },
          { image: 'field11.webp' },
        ],
        backgroundChromeImage: 'chrome1.webp',
        useSoundFx: true,
        soundVolume: 5,
        sacle: scale,
        useGrid: false,
        showFlat: false,
        showHints: false,
        showSettings: false,
        highlightPlayerHand: false,
        curtainLeftText: 'Waiting for your opponent&nbsp;',
        curtainRightText: 'to step into the arena...',
        showModal: false,
        searchValue: "",
        transformMatrix: {},
        inverseTransformMatrix: {},
        screenTransformMatrix: {},
        inverseScreenTransformMatrix: {},
        perspectiveDragOriginLeft: 0,
        perspectiveDragOriginTop: 0,
        perspectiveDragCssPostion: 'relative',
        previewImageSrc: "",
        preview: false,
        gameGuid: null,
        isStarted: false,
        coinflip: false,
        heads: true,
        diceRoll: false,
        cards: [],
        actions: [],
        player: {
            showCurtain: true,
            name: null,
            playerGuid: null,
            deckGuid: null,
            statsTimer: null,
            deck: [],
            play: [],
            hand: [],
            removed: [],
            graveyard: [],
            VictoryPoints: 15,
            resources: {
                black: { used: 0, available: 0 },
                yellow: { used: 0, available: 0 },
                green: { used: 0, available: 0 },
                blue: { used: 0, available: 0 },
                red: { used: 0, available: 0 },
            }
        },
        opponent: {
            name: "waiting...",
            showCurtain: true,
            playerGuid: null,
            deckGuid: null,
            statsTimer: null,
            deck: [],
            play: [],
            hand: [],
            removed: [],
            graveyard: [],
            VictoryPoints: 15,
            resources: {
                black: { used: 0, available: 0 },
                yellow: { used: 0, available: 0 },
                green: { used: 0, available: 0 },
                blue: { used: 0, available: 0 },
                red: { used: 0, available: 0 },
            }
        },
        cardlist: {
            show: false,
            cards: [],
            sync: {
                playerGuid: null,
                kind: null,
                mustsync: false,
            },
            oldIndex: -1,
        },
        services: {
            login: null,
            logout: null,
            users: null,
            cards: null,
            cardSearch: null,
            decks: null,
            deckSearch: null,
            deckCards: null,
            cardOptions: null,
            gameSearch: null,
            games: null,
            game: null,
            jitsiApi: null,
        },
        newChatMessage: null,
        chatMessages:[],
        jitsi: {
            isVideoMuted: true,
            isAudioMuted: true,
            domain:  'meet.jit.si',
            options: {
                roomName: 'JitsiMeetAPIExampleTest',
                width:'100%',
                height: '100%',
                parentNode: document.querySelector('#jitsi'),
                lang: 'en',
                userInfo: {
                    displayName: 'player',
                    email: 'player.webp'
                },
                interfaceConfigOverwrite :{
                    SHOW_CHROME_EXTENSION_BANNER: false,
                    // Determines how the video would fit the screen. 'both' would fit the whole
                    // screen, 'height' would fit the original video height to the height of the
                    // screen, 'width' would fit the original video width to the width of the
                    // screen respecting ratio, 'nocrop' would make the video as large as
                    // possible and preserve aspect ratio without cropping.
                    VIDEO_LAYOUT_FIT: 'both',
                    SET_FILMSTRIP_ENABLED: false,
                },
                configOverwrite: {
                    disableModeratorIndicator: true,
                    disableReactions: true,
                    disableReactionsModeration: true,
                    disablePolls: true,
                    disableSelfView: true,
                    disableSelfViewSettings: true,
                    startAudioOnly: false,
                    startWithAudioMuted: true,
                    disableResponsiveTiles: true,
                    hideLobbyButton: true,
                    autoKnockLobby: true,
                    enableLobbyChat: false,
                    requireDisplayName: false,
                    enableWelcomePage: false,
                    disableShortcuts: true,
                    enableClosePage: true,
                    defaultLocalDisplayName: 'me',
                    defaultRemoteDisplayName: 'opponent',
                    hideDisplayName: true,
                    hideDominantSpeakerBadge: true,
                    defaultLanguage: 'en',
                    disableProfile: true,
                    hideEmailInSettings: true,
                    prejoinConfig: {
                        enabled: false,
                        hideDisplayName: true,
                        hideExtraJoinButtons: ['no-audio', 'by-phone']
                    },
                    readOnlyName: true,
                    enableInsecureRoomNameWarning: false,
                    enableAutomaticUrlCopy: false,
                    // Array with avatar URL prefixes that need to use CORS.                    
                    corsAvatarURLs: [ 'https://aminduna.arcmage.org/arcmage/Game/' ],
                    gravatarBaseURL: 'https://aminduna.arcmage.org/arcmage/Game/',
                    // Setup for Gravatar-compatible services.
                    gravatar: {
                        // Defaults to Gravatar.
                        baseUrl: 'https://aminduna.arcmage.org/arcmage/Game/',
                        // True if Gravatar should be disabled.
                        disabled: true
                    },
                    
                    toolbarButtons: [ ],
                    toolbarConfig: {
                         alwaysVisible: false,
                         autoHideWhileChatIsOpen: true
                    },
                    hiddenPremeetingButtons: ['microphone', 'camera', 'select-background', 'invite', 'settings'],
                    disableThirdPartyRequests: true,
                    notifications: [],
                    disabledSounds: [
                        'ASKED_TO_UNMUTE_SOUND',
                        'E2EE_OFF_SOUND',
                        'E2EE_ON_SOUND',
                        'INCOMING_MSG_SOUND',
                        'KNOCKING_PARTICIPANT_SOUND',
                        'LIVE_STREAMING_OFF_SOUND',
                        'LIVE_STREAMING_ON_SOUND',
                        'NO_AUDIO_SIGNAL_SOUND',
                        'NOISY_AUDIO_INPUT_SOUND',
                        'OUTGOING_CALL_EXPIRED_SOUND',
                        'OUTGOING_CALL_REJECTED_SOUND',
                        'OUTGOING_CALL_RINGING_SOUND',
                        'OUTGOING_CALL_START_SOUND',
                        'PARTICIPANT_JOINED_SOUND',
                        'PARTICIPANT_LEFT_SOUND',
                        'RAISE_HAND_SOUND',
                        'REACTION_SOUND',
                        'RECORDING_OFF_SOUND',
                        'RECORDING_ON_SOUND',
                        'TALK_WHILE_MUTED_SOUND'
                    ],
                    disableInitialGUM: true,
                    
                    // resolution: 360,
                    // constraints: {
                    //     video: {
                    //         height: {
                    //             ideal: 360,
                    //             max: 360,
                    //             min: 240
                    //         }
                    //     }
                    // },
                    // disableSimulcast: true,
                    // channelLastN: -1,
                    // startLastN: 10,
                    // resolution: 180,
                    // maxFps: 15,
                    // e2eping: {pingInterval: -1},
                    // desktopSharingFrameRate: {min:5, max:30},
                    // constraints: {
                    //     video: {
                    //         aspectRatio: 16 / 9,
                    //         frameRate: {
                    //             max: 15
                    //         },
                    //         height: {
                    //             ideal: 180,
                    //             max: 180,
                    //             min: 180
                    //         }
                    //     }
                    // },
                    // // Specify the settings for video quality optimizations on the client.
                    // videoQuality: {
                    //    // Provides a way to prevent a video codec from being negotiated on the JVB connection. The codec specified
                    //    // here will be removed from the list of codecs present in the SDP answer generated by the client. If the
                    //    // same codec is specified for both the disabled and preferred option, the disable settings will prevail.
                    //    // Note that 'VP8' cannot be disabled since it's a mandatory codec, the setting will be ignored in this case.
                    //    // disabledCodec: 'H264',
                    
                    //    // Provides a way to set a preferred video codec for the JVB connection. If 'H264' is specified here,
                    //    // simulcast will be automatically disabled since JVB doesn't support H264 simulcast yet. This will only
                    //    // rearrange the the preference order of the codecs in the SDP answer generated by the browser only if the
                    //    // preferred codec specified here is present. Please ensure that the JVB offers the specified codec for this
                    //    // to take effect.
                    //    preferredCodec: 'VP8',
                    
                    //    // Provides a way to enforce the preferred codec for the conference even when the conference has endpoints
                    //    // that do not support the preferred codec. For example, older versions of Safari do not support VP9 yet.
                    //    // This will result in Safari not being able to decode video from endpoints sending VP9 video.
                    //    // When set to false, the conference falls back to VP8 whenever there is an endpoint that doesn't support the
                    //    // preferred codec and goes back to the preferred codec when that endpoint leaves.
                    //    // enforcePreferredCodec: false,
                    
                    //    // Provides a way to configure the maximum bitrates that will be enforced on the simulcast streams for
                    //    // video tracks. The keys in the object represent the type of the stream (LD, SD or HD) and the values
                    //    // are the max.bitrates to be set on that particular type of stream. The actual send may vary based on
                    //    // the available bandwidth calculated by the browser, but it will be capped by the values specified here.
                    //    // This is currently not implemented on app based clients on mobile.
                    //    maxBitratesVideo: {
                    //          H264: {
                    //              low: 200000,
                    //              standard: 500000,
                    //              high: 1500000
                    //          },
                    //          VP8 : {
                    //              low: 200000,
                    //              standard: 500000,
                    //              high: 1500000
                    //          },
                    //          VP9: {
                    //              low: 100000,
                    //              standard: 300000,
                    //              high: 1200000
                    //          }
                    //    },
                    
                    //    // The options can be used to override default thresholds of video thumbnail heights corresponding to
                    //    // the video quality levels used in the application. At the time of this writing the allowed levels are:
                    //    //     'low' - for the low quality level (180p at the time of this writing)
                    //    //     'standard' - for the medium quality level (360p)
                    //    //     'high' - for the high quality level (720p)
                    //    // The keys should be positive numbers which represent the minimal thumbnail height for the quality level.
                    //    //
                    //    // With the default config value below the application will use 'low' quality until the thumbnails are
                    //    // at least 360 pixels tall. If the thumbnail height reaches 720 pixels then the application will switch to
                    //    // the high quality.
                    //    minHeightForQualityLvl: {
                    //        10: 'low',                           
                    //        45: 'standard',
                    //        90: 'high',
                    //    },
                    
                    //    // Provides a way to resize the desktop track to 720p (if it is greater than 720p) before creating a canvas
                    //    // for the presenter mode (camera picture-in-picture mode with screenshare).
                    //    resizeDesktopForPresenter: false
                    // },
                    // useNewBandwidthAllocationStrategy: false,
                    disableInviteFunctions: true,
                    remoteVideoMenu: {
                            disabled: true,
                    },
                    disableRemoteMute: true,
                    participantsPane: {
                        // Hides the moderator settings tab.
                        hideModeratorSettingsTab: true,
                        // Hides the more actions button.
                        hideMoreActionsButton: true,
                        // Hides the mute all button.
                        hideMuteAllButton: true
                    },
                    disableAddingBackgroundImages: true,
                    disableTileView: true,
                },
                // Controls the visibility and behavior of the top header conference info labels.
                // If a label's id is not in any of the 2 arrays, it will not be visible at all on the header.
                conferenceInfo: {
                    // those labels will not be hidden in tandem with the toolbox.
                    alwaysVisible: [],
                    // those labels will be auto-hidden in tandem with the toolbox buttons.
                    autoHide: [
                        // 'subject',
                        // 'conference-timer',
                        // 'participants-count',
                        // 'e2ee',
                        // 'transcribing',
                        // 'video-quality',
                        // 'insecure-room',
                        // 'highlight-moment'
                    ]
                },
                // Hides the conference subject
                hideConferenceSubject: true,
                // Hides the conference timer.
                hideConferenceTimer: true,
                // Hides the recording label
                hideRecordingLabel: true,
                // Hides the participants stats
                hideParticipantsStats: true,
                // Sets the conference subject
                subject: '',
                //Sets the conference local subject
                localSubject: '',
                
                // filmstrip: {
                //     // Disables user resizable filmstrip. Also, allows configuration of the filmstrip
                //     // (width, tiles aspect ratios) through the interfaceConfig options.
                //     disableResizable: true,

                //     // Disables the stage filmstrip
                //     // (displaying multiple participants on stage besides the vertical filmstrip)
                //     disableStageFilmstrip: false
                // },
            }
        }
    },
    ready: function() {
      
        // create rest services
        this.createServices();
        // Add watcher for when the modal dialog closes,
        // - syncs the card visible state
        this.$watch('cardlist.show',
            function(value) {
                if (!value) {
                    vue.syncCardList();
                    setDroppableState(true);
                }
            });
        // Apply the layouting (matrix-3d transform, position victory point sliders)
        $(window).on('resize', function(e) { 
            var fieldView = sizing.fieldview.perspective;
            if(vue.showFlat) { fieldView = sizing.fieldview.flat; }
            resizeGame(sizing.battlefield.width, sizing.battlefield.height, fieldView.distancePercentage, fieldView.rightOffset, fieldView.leftOffsetPercentage); 
        }, 1000).resize();
        // Set up the droppable regions on the battlefield
        setupDropRegions();
      
        // Start the game's BL
        $(init);
    },
    computed: {
        filteredCards: function() {
            var search = this.searchValue.toLowerCase();
            return this.cardlist.cards.filter(function(c) {
                if (search === "" || search.length < 3) return true;
                return c.name.toLowerCase().indexOf(search) >= 0 ||
                    c.ruleText.toLowerCase().indexOf(search) >= 0;
            });
        }
    },
    methods: {
        restoreSettings: function(){
            var settingsJson = window.localStorage.getItem("settings");
            if (settingsJson){
                var settings = JSON.parse(settingsJson);
                if (settings.showFlat) {
                    this.setPerspective(settings.showFlat, false);
                }
                if (settings.backgroundImage){
                    this.setBackground(settings.backgroundImage, false);
                }
                this.setSoundFx(settings.useSoundFx, false);
                this.setSoundVolume(settings.soundVolume, false);
            }
        },
        saveSettings: function(){
            var settings = {
                showFlat: vue.showFlat,
                backgroundImage: vue.backgroundImage,
                useSoundFx: vue.useSoundFx,
                soundVolume: vue.soundVolume
            };
            window.localStorage.setItem('settings', JSON.stringify(settings));
        },
        openCurtain: function() {
            vue.player.showCurtain = false;
            if (vue.isStarted) {
                sendGameAction({
                    gameGuid: vue.gameGuid,
                    playerGuid: vue.player.playerGuid,
                    actionType: 'changeCurtainState',
                    actionData: {
                        playerGuid: vue.player.playerGuid,
                        showCurtain: false
                    }
                });
            }
        },
        setCurtainState: function (player, state) {
            if (vue.isStarted) {
                sendGameAction({
                    gameGuid: vue.gameGuid,
                    playerGuid: vue.player.playerGuid,
                    actionType: 'changeCurtainState',
                    actionData: {
                        playerGuid: player.playerGuid,
                        showCurtain: state
                    }
                });
            }
        },
        setBackground: function(image, save) {
            vue.backgroundImage = image;
            if(save){
                this.saveSettings();
            }
        },
        setSoundFx: function(useSoundFx, save){
            vue.useSoundFx = useSoundFx;
            sound.mute(!useSoundFx);
            soundIntro.mute(!useSoundFx);
            if(save){
                this.saveSettings();
            }
        },
        setSoundVolume: function(soundVolume, save) {
            if(!soundVolume) soundVolume = 5;
            vue.soundVolume = soundVolume;
            sound.volume(soundVolume / 10.0);
            soundIntro.volume( (soundVolume / 10.0) * 0.6);
            if(save){
                this.saveSettings();
            }
        },
        checkVolumeLevel: function(level){
            if(this.soundVolume) {
                return this.useSoundFx && level <= this.soundVolume;
            }
            return false;
        },
        openVideo: function() {
           // window.open("https://brie.fi/ng/arcmage_" + vue.gameGuid, "_blank");
		   window.open("https://meet.jit.si/arcmage_" + vue.gameGuid, "_blank");
        },
        toggleVideo: function(){
            vue.services.jitsiApi.executeCommand('toggleVideo');
        },
        toggleAudio: function(){
            vue.services.jitsiApi.executeCommand('toggleAudio');
        },
        sendChatMessage(){
            if(vue.newChatMessage){
                vue.services.jitsiApi.executeCommand('sendChatMessage', vue.newChatMessage, null, false);
                vue.newChatMessage = null;
            }
        },
        openRules: function () {
            window.open(portalUri + "/arcmage/Game/pdfjs/web/viewer.html?file=" + portalUri + "/arcmage/Game/ArcmageRules.pdf", "_blank");
        },
        openHints: function(){
            vue.showHints = true;
        },
        openSettings: function(){
            vue.showSettings = true;
        },
        flipCoin: function() {
            if (!vue.coinflip) {
                sendGameAction({
                    gameGuid: vue.gameGuid,
                    playerGuid: vue.player.playerGuid,
                    actionType: 'flipCoin',
                });
            }
        },
        rollDice: function() {

            if (!vue.diceRoll) {
                sendGameAction({
                    gameGuid: vue.gameGuid,
                    playerGuid: vue.player.playerGuid,
                    actionType: 'diceRoll',
                });
            }
        },
        setPerspective:function (flat, save){
            vue.showFlat = flat;
            var fieldView = sizing.fieldview.perspective;
            if(vue.showFlat) { fieldView = sizing.fieldview.flat; }
            resizeGame(sizing.battlefield.width, sizing.battlefield.height, fieldView.distancePercentage, fieldView.rightOffset, fieldView.leftOffsetPercentage); 
            if(save){
                this.saveSettings();
            }
        },
        createServices: function() {
            this.services.login = this.$resource(apiUri + "/api/Login");
            this.services.logout = this.$resource(apiUri + "/api/Logout");
            this.services.users = this.$resource(apiUri + "/api/Users{/guid}");
            this.services.cards = this.$resource(apiUri + "/api/Cards{/guid}");
            this.services.cardSearch = this.$resource(apiUri + "/api/CardSearch");
            this.services.decks = this.$resource(apiUri + "/api/Decks{/guid}");
            this.services.deckSearch = this.$resource(apiUri + "/api/DeckSearch");
            this.services.deckCards = this.$resource(apiUri + "/api/DeckCards");
            this.services.cardOptions = this.$resource(apiUri + "/api/CardOptions{/guid}");
            this.services.deckOptions = this.$resource(apiUri + "/api/DeckOptions{/guid}");

            this.services.gameSearch = this.$resource(gameApiUri + "/api/GameSearch");
            this.services.games = this.$resource(gameApiUri + "/api/Games");
        },
        toggleMark: function(card) {
            sendGameAction({
                gameGuid: vue.gameGuid,
                playerGuid: vue.player.playerGuid,
                actionType: 'changeCardState',
                actionData: {
                    cardId: card.cardId,
                    isMarked: !card.isMarked,
                }
            });
        },
        toggleFaceDown: function(card) {
            sendGameAction({
                gameGuid: vue.gameGuid,
                playerGuid: vue.player.playerGuid,
                actionType: 'changeCardState',
                actionData: {
                    cardId: card.cardId,
                    isFaceDown: !card.isFaceDown,
                }
            });
        },
        increaseCounter: function(card, kind) {
            if (kind === 'counterA') {
                sendGameAction({
                    gameGuid: vue.gameGuid,
                    playerGuid: vue.player.playerGuid,
                    actionType: 'changeCardState',
                    actionData: {
                        cardId: card.cardId,
                        counterA: card.counterA + 1,
                    }
                });
            }
            if (kind === 'counterB') {
                sendGameAction({
                    gameGuid: vue.gameGuid,
                    playerGuid: vue.player.playerGuid,
                    actionType: 'changeCardState',
                    actionData: {
                        cardId: card.cardId,
                        counterB: card.counterB + 1,
                    }
                });
            }
        },
        decreaseCounter: function(card, kind) {
            if (kind === 'counterA') {
                var newCounterA = card.counterA - 1;
                if (newCounterA < 0) newCounterA = 0;
                sendGameAction({
                    gameGuid: vue.gameGuid,
                    playerGuid: vue.player.playerGuid,
                    actionType: 'changeCardState',
                    actionData: {
                        cardId: card.cardId,
                        counterA: newCounterA,
                    }
                });
            }
            if (kind === 'counterB') {
                var newCounterB = card.counterB - 1;
                if (newCounterB < 0) newCounterB = 0;
                sendGameAction({
                    gameGuid: vue.gameGuid,
                    playerGuid: vue.player.playerGuid,
                    actionType: 'changeCardState',
                    actionData: {
                        cardId: card.cardId,
                        counterB: newCounterB,
                    }
                });
            }
        },
        counterWheel: function(event, card, kind) {
            var e = window.event || event; // old IE support
            var delta = Math.max(-1, Math.min(1, -e.deltaY));
            if (delta > 0) {
                this.increaseCounter(card, kind);
            } else {
                this.decreaseCounter(card, kind);
            };
            return false;
        },
        moveCardFrom: function(actionType, fromPlayerGuid, fromKind, toPlayerGuid, toKind, isFaceDown, times = 1) {
            var i;
            for (i = 0; i < times; i++) {
                sendGameAction({
                    gameGuid: vue.gameGuid,
                    playerGuid: vue.player.playerGuid,
                    actionType: actionType,
                    actionData: {
                        fromPlayerGuid: fromPlayerGuid,
                        fromKind: fromKind,
                        toPlayerGuid: toPlayerGuid,
                        toKind: toKind,
                        cardState: {
                            isFaceDown: isFaceDown,
                        }
                    }
                });
            }
        },
        handleGlobalKeyPress: function() {
            // not handling keyboard events in modals
            if (this.showCurtain || this.showHints || this.showModal || this.showSettings || this.cardlist.show) return;
 
            switch(this.keycode){
                case "Ctrl+ ":
                case "Ctrl+Enter":
                    // unmark all
                    this.unmarkAll(this.player);
                    break;
                case "Shift+ ":
                case "Shift+Enter":
                    // unmark all
                    this.unmarkAll(this.opponent);
                    break;
                case "Ctrl+d":
                case "Ctrl+D":
                    // draw one cards
                    this.moveCardFrom('drawCard', this.player.playerGuid, 'deck', this.player.playerGuid, 'hand', false);
                    break;
                case "Ctrl+t":
                case "Ctrl+T":
                    // draw two cards
                    this.moveCardFrom('drawCard', this.player.playerGuid, 'deck', this.player.playerGuid, 'hand', false, 2);
                break;
                case "Ctrl+r":
                case "Ctrl+R":
                    // roll a dice
                    this.rollDice();
                    break;
                case "Ctrl+b":
                case "Ctrl+B":
                    // toggle opponent blinds
                    this.setCurtainState(this.opponent, !this.opponent.showCurtain);
                    break;
                case "Ctrl+f":
                case "Ctrl+F":
                    // flip a coin
                    this.flipCoin();
                    break;
            }
        },
        handMouseCardAction: function(event, card, fromPlayer) {

            if (this.showCurtain || this.showHints || this.showModal || this.showSettings || this.cardlist.show) return;

            switch(this.keycode){
                case "g":
                case "G":
                    // put card in graveyard
                    sendGameAction({
                        gameGuid: vue.gameGuid,
                        playerGuid: vue.player.playerGuid,
                        actionType: 'discardCard',
                        actionData: {
                            fromPlayerGuid: fromPlayer.playerGuid,
                            fromKind: 'hand',
                            toPlayerGuid: fromPlayer.playerGuid,
                            toKind: 'graveyard',
                            cardId: card.cardId,
                            cardState: {
                                cardId: card.cardId,
                                isFaceDown: false,
                                top: 0,
                                left: 0
                            }
                        }
                    });
                    break;
                case "m":
                case "M":
                case "x":
                case "X":
                    // mark or unmark card
                    this.toggleMark(card);
                    break;
                case "u":
                case "U":
                    // toggle face down
                    this.toggleFaceDown(card)
                    break;
            }
        },
        mouseCardAction: function(event, card, fromPlayer) {
            // peek card
            if (event.shiftKey) {
                this.peekCard(card);
            }
            // point card
            if (event.ctrlKey) {
               this.pointCard(card);
            }

            switch(this.keycode){
                case "g":
                case "G":
                    // put card in graveyard
                    sendGameAction({
                        gameGuid: vue.gameGuid,
                        playerGuid: vue.player.playerGuid,
                        actionType: 'discardCard',
                        actionData: {
                            fromPlayerGuid: fromPlayer.playerGuid,
                            fromKind: 'play',
                            toPlayerGuid: fromPlayer.playerGuid,
                            toKind: 'graveyard',
                            cardId: card.cardId,
                            cardState: {
                                cardId: card.cardId,
                                isFaceDown: false,
                                top: 0,
                                left: 0
                            }
                        }
                    });

                    break;
                case "h":
                case "H":
                    // put card in hand of its controller
                    sendGameAction({
                        gameGuid: vue.gameGuid,
                        playerGuid: vue.player.playerGuid,
                        actionType: 'drawCard',
                        actionData: {
                            fromPlayerGuid: fromPlayer.playerGuid,
                            fromKind: 'play',
                            toPlayerGuid: fromPlayer.playerGuid,
                            toKind: 'hand',
                            cardId: card.cardId,
                            cardState: {
                                cardId: card.cardId,
                                isFaceDown: false,
                                top: 0,
                                left: 0
                            }
                        }
                    });
                case "c":
                case "C":
                    // take control of the card (become the controller/owner)
                    if (fromPlayer.playerGuid != this.player.playerGuid) {                      
                        sendGameAction({
                            gameGuid: vue.gameGuid,
                            playerGuid: vue.player.playerGuid,
                            actionType: 'playCard',
                            actionData: {
                                fromPlayerGuid: fromPlayer.playerGuid,
                                fromKind: 'play',
                                toPlayerGuid: this.player.playerGuid,
                                toKind: 'play',
                                cardId: card.cardId,
                                cardState: {
                                    cardId: card.cardId,
                                    top: card.top,
                                    left: card.left
                                }
                            }
                        });
                        this.pointCard(card);

                    }
                    break;
                case "p":
                case "P":
                    // point at a card
                    this.pointCard(card);
                    break;
                case "l":
                    case "L":
                    // look at a facedown card
                    this.peekCard(card);
                    break;
                case "m":
                case "M":
                case "x":
                case "X":
                    // mark or unmark card
                    this.toggleMark(card);
                    break;
                case "u":
                case "U":
                    // toggle face down
                    this.toggleFaceDown(card)
                    break;
            }
        },
        pointCard: function(card){
            card.isPointed = true;
                // send game action point card.
                sendGameAction({
                    gameGuid: vue.gameGuid,
                    playerGuid: vue.player.playerGuid,
                    actionType: 'changeCardState',
                    actionData: {
                        cardId: card.cardId,
                        isPointed: true,
                    }
                });

                setTimeout((unPointCard) => {
                    unPointCard.isPointed = false;
                    // send game action point card.
                    sendGameAction({
                        gameGuid: vue.gameGuid,
                        playerGuid: vue.player.playerGuid,
                        actionType: 'changeCardState',
                        actionData: {
                            cardId: unPointCard.cardId,
                            isPointed: false,
                        }
                    });
                }, 2600, card);
        },
        peekCard: function(card){
            if (card.isFaceDown) {
                this.previewImageSrc = card.imageSrc;
                this.preview = true;
                card.isPeeking = true;
                // send game action peek card.
                sendGameAction({
                    gameGuid: vue.gameGuid,
                    playerGuid: vue.player.playerGuid,
                    actionType: 'changeCardState',
                    actionData: {
                        cardId: card.cardId,
                        isPeeking: true,
                    }
                });
            }
        },
        showPreview: function (card) {
            if (!card.isFaceDown) {
                this.previewImageSrc = card.imageSrc;
                this.preview = true;
            } else {
                this.preview = false;
            }
        },
        hidePreview: function (card) {
            this.preview = false;
            card.isPeeking = false;
           // card.isPointed = false;
            sendGameAction({
                gameGuid: vue.gameGuid,
                playerGuid: vue.player.playerGuid,
                actionType: 'changeCardState',
                actionData: {
                    cardId: card.cardId,
                    isPeeking: false,
             //       isPointed: false,
                }
            });
        },
        increaseAvailableResource: function(playerGuid, resource) {
            if (resource.available < 99) {
                resource.available++;
                updatePlayerStatsAction(playerGuid);
            }
        },
        decreaseAvailableResource: function(playerGuid, resource) {
            if (resource.available > 0) {
                resource.available--;
                updatePlayerStatsAction(playerGuid);
            }
        },
        availableResourceWheel: function(event, playerGuid, resource) {
            var e = window.event || event; // old IE support
            var delta = Math.max(-1, Math.min(1, -e.deltaY));
            if (delta > 0) {
                this.increaseAvailableResource(playerGuid, resource);

            } else {
                this.decreaseAvailableResource(playerGuid, resource);
            };
            return false;
        },
        increaseUsedResource: function(playerGuid, resource) {
            if (resource.used < resource.available) {
                resource.used++;
                updatePlayerStatsAction(playerGuid);
            }
        },
        decreaseUsedResource: function(playerGuid, resource) {
            if (resource.used > 0) {
                resource.used--;
                updatePlayerStatsAction(playerGuid);
            }
        },
        usedResourceWheel: function(event, playerGuid, resource) {
            var e = window.event || event; // old IE support
            var delta = Math.max(-1, Math.min(1, -e.deltaY));
            if (delta > 0) {
                this.increaseUsedResource(playerGuid, resource);
            } else {
                this.decreaseUsedResource(playerGuid, resource);
            };
            return false;
        },
        playResourceCard: function(card, kind) {

            switch (kind) {
                case 'black':
                    vue.player.resources.black.used++;
                    vue.player.resources.black.available++;
                    break;
                case 'blue':
                    vue.player.resources.blue.used++;
                    vue.player.resources.blue.available++;
                    break;
                case 'red':
                    vue.player.resources.red.used++;
                    vue.player.resources.red.available++;
                    break;
                case 'green':
                    vue.player.resources.green.used++;
                    vue.player.resources.green.available++;
                    break;
                case 'yellow':
                    vue.player.resources.yellow.used++;
                    vue.player.resources.yellow.available++;
                    break;
            }

            sendGameAction({
                gameGuid: vue.gameGuid,
                playerGuid: vue.player.playerGuid,
                actionType: 'changePlayerStats',
                actionData: {
                    playerGuid: vue.player.playerGuid,
                    victoryPoints: vue.player.VictoryPoints,
                    resources: {
                        black: {
                            used: vue.player.resources.black.used,
                            available: vue.player.resources.black.available,
                        },
                        blue: {
                            used: vue.player.resources.blue.used,
                            available: vue.player.resources.blue.available,
                        },
                        red: {
                            used: vue.player.resources.red.used,
                            available: vue.player.resources.red.available,
                        },
                        green: {
                            used: vue.player.resources.green.used,
                            available: vue.player.resources.green.available,
                        },
                        yellow: {
                            used: vue.player.resources.yellow.used,
                            available: vue.player.resources.yellow.available,
                        }
                    }
                }
            });

            var totalResources =
                vue.player.resources.black.available +
                vue.player.resources.blue.available +
                vue.player.resources.red.available +
                vue.player.resources.green.available +
                vue.player.resources.yellow.available;

            var top = sizing.battlefield.height - sizing.autoResourceGap - sizing.card.height - 3 * totalResources;
            var left = sizing.autoResourceGap + 3 * totalResources;

            sendGameAction({
                gameGuid: vue.gameGuid,
                playerGuid: vue.player.playerGuid,
                actionType: 'playCard',
                actionData: {
                    fromPlayerGuid: vue.player.playerGuid,
                    fromKind: 'hand',
                    toPlayerGuid: vue.player.playerGuid,
                    toKind: 'play',
                    cardId: card.cardId,
                    cardState: {
                        cardId: card.cardId,
                        isFaceDown: true,
                        top: top,
                        left: left
                    }
                }
            });
        },
        shuffleCards: function (playerGuid, kind) {
            sendGameAction({
                gameGuid: vue.gameGuid,
                playerGuid: vue.player.playerGuid,
                actionType: 'shuffleList',
                actionData: {
                    playerGuid: playerGuid,
                    kind: kind,
                }
            });
        },
        unmarkAll: function(player) {
           
            sound.play('rotateCard');

            var actionData = {
                playerGuid: player.playerGuid,
                kind: 'Play',
                cards: []
            };
            $.each(player.play, function (index, card) {
                card.isMarked = false;
                actionData.cards.push({
                    cardId: card.cardId,
                    isMarked: card.isMarked
                });
            });
            sendGameAction({
                gameGuid: vue.gameGuid,
                playerGuid: vue.player.playerGuid,
                actionType: 'updateList',
                actionData: actionData
            });
            vue.player.resources.black.used = vue.player.resources.black.available;
            vue.player.resources.blue.used = vue.player.resources.blue.available;
            vue.player.resources.red.used = vue.player.resources.red.available;
            vue.player.resources.green.used = vue.player.resources.green.available;
            vue.player.resources.yellow.used = vue.player.resources.yellow.available;

            sendGameAction({
                gameGuid: vue.gameGuid,
                playerGuid: vue.player.playerGuid,
                actionType: 'changePlayerStats',
                actionData: {
                    playerGuid: vue.player.playerGuid,
                    victoryPoints: vue.player.VictoryPoints,
                    resources: {
                        black: {
                            used: vue.player.resources.black.used,
                            available: vue.player.resources.black.available,
                        },
                        blue: {
                            used: vue.player.resources.blue.used,
                            available: vue.player.resources.blue.available,
                        },
                        red: {
                            used: vue.player.resources.red.used,
                            available: vue.player.resources.red.available,
                        },
                        green: {
                            used: vue.player.resources.green.used,
                            available: vue.player.resources.green.available,
                        },
                        yellow: {
                            used: vue.player.resources.yellow.used,
                            available: vue.player.resources.yellow.available,
                        }
                    }
                }
            });

        },

        showCardList: function (playerGuid, kind) {
            this.searchValue = "";                       
            setDroppableState(false);
            this.cardlist.sync.mustsync = true;
            this.cardlist.cards = [];
            this.cardlist.sync.playerGuid = playerGuid;
            this.cardlist.sync.kind = kind;
            var isFaceDown = kind !== 'graveyard';
            var list = getList(playerGuid, kind);
            $.each(list, function (index, card) {
                vue.cardlist.cards.push({
                    cardId: card.cardId,
                    name: card.name,
                    ruleText: card.ruleText,
                    imageSrc: card.imageSrc,
                    isFaceDown: isFaceDown,
                    isSelected: false,
                });
            });
            this.cardlist.cards.reverse();

            $("#cardlist").sortable({
                start: function (event, ui) {
                    vue.cardlist.oldIndex = ui.item.index();
                },
              
                stop: function (event, ui) {
                    var newIndex = ui.item.index();
                    vue.swapCards(vue.cardlist.cards, vue.cardlist.oldIndex, newIndex);
                    vue.cardlist.oldIndex = newIndex;
                },
            });
            this.cardlist.show = true;
        },
        syncCardList: function () {
            if (!this.cardlist.sync.mustsync) return;
            var actionData = {
                playerGuid: this.cardlist.sync.playerGuid,
                kind: this.cardlist.sync.kind,
                cards: []
            };

            this.cardlist.cards.reverse();

            $.each(this.cardlist.cards, function (index, card) {
                actionData.cards.push({
                    cardId: card.cardId,
                    isFaceDown: card.isFaceDown
                });
            });
            sendGameAction({
                gameGuid: vue.gameGuid,
                playerGuid: vue.player.playerGuid,
                actionType: 'updateList',
                actionData: actionData
            });
            
        },
        selectFromCardList: function(card, event) {
            if (!event.ctrlKey) {
                $.each(this.cardlist.cards, function (index, card) {
                    card.isSelected = false;
                });
                card.isSelected = true;
            }
        },
        multiSelectFromCardList: function (card, event) {
            if (event.ctrlKey) {
                card.isSelected = !card.isSelected;
            }
        },
        setCardListFaceDown(faceDown) {
            $.each(this.cardlist.cards, function (index, card) {
                card.isFaceDown = faceDown;
            });
        },
        shuffleCardList: function () {
            var currentIndex = this.cardlist.cards.length, randomIndex;
            while (0 !== currentIndex) {
                randomIndex = Math.floor(Math.random() * currentIndex);
                currentIndex -= 1;
                this.swapCards(this.cardlist.cards, currentIndex, randomIndex);
            }
        },
        swapCards: function (list, oldIndex, newIndex) {
            list.splice(newIndex, 0, list.splice(oldIndex, 1)[0]);
        },
        toggleCardListFaceDown: function(card) {
            card.isFaceDown = !card.isFaceDown;
        },
        cardlistDrawCard: function () {

            if (!(this.cardlist.sync.kind === 'hand' &&
                this.cardlist.sync.playerGuid === vue.player.playerGuid)) {

                var fromPlayerGuid = this.cardlist.sync.playerGuid;
                var fromKind = this.cardlist.sync.kind;

                $.each(this.cardlist.cards, function (index, card) {
                    if (card.isSelected) {
                        sendGameAction({
                            gameGuid: vue.gameGuid,
                            playerGuid: vue.player.playerGuid,
                            actionType: 'drawCard',
                            actionData: {
                                fromPlayerGuid: fromPlayerGuid,
                                fromKind: fromKind,
                                toPlayerGuid: vue.player.playerGuid,
                                toKind: 'hand',
                                cardId: card.cardId,
                                cardState: {
                                    cardId: card.cardId,
                                    isFaceDown: card.isFaceDown,
                                }
                            }
                        });
                    }
                });

               
                this.cardlist.sync.mustsync = false;
                this.cardlist.show = false;
            }
            
        },
        cardlistDiscardCard: function () {
            if (this.cardlist.sync.kind !== 'graveyard') {

                var playerGuid = this.cardlist.sync.playerGuid;
                var fromKind = this.cardlist.sync.kind;

                $.each(this.cardlist.cards, function(index, card) {
                    if (card.isSelected) {
                        sendGameAction({
                            gameGuid: vue.gameGuid,
                            playerGuid: vue.player.playerGuid,
                            actionType: 'discardCard',
                            actionData: {
                                fromPlayerGuid: playerGuid,
                                fromKind: fromKind,
                                toPlayerGuid: playerGuid,
                                toKind: 'graveyard',
                                cardId: card.cardId,
                                cardState: {
                                    cardId: card.cardId,
                                    isFaceDown: false,
                                }
                            }
                        });
                    }
                });
                this.cardlist.sync.mustsync = false;
                this.cardlist.show = false;
            }
            
        },
        cardlistPlayCard: function () {
            if (this.cardlist.sync.kind !== 'play') {

                var playerGuid = this.cardlist.sync.playerGuid;
                var fromKind = this.cardlist.sync.kind;

                $.each(this.cardlist.cards, function(index, card) {
                    if (card.isSelected) {
                        sendGameAction({
                            gameGuid: vue.gameGuid,
                            playerGuid: vue.player.playerGuid,
                            actionType: 'playCard',
                            actionData: {
                                fromPlayerGuid: playerGuid,
                                fromKind: fromKind,
                                toPlayerGuid: playerGuid,
                                toKind: 'play',
                                cardId: card.cardId,
                                cardState: {
                                    cardId: card.cardId,
                                    isFaceDown: card.isFaceDow,
                                }
                            }
                        });
                    }
                });
             
                this.cardlist.sync.mustsync = false;
                this.cardlist.show = false;
            }
            
        },
        cardlistDeckCard: function () {

            var playerGuid = this.cardlist.sync.playerGuid;
            var fromKind = this.cardlist.sync.kind;

            if (this.cardlist.sync.kind !== 'deck') {
                $.each(this.cardlist.cards, function(index, card) {
                    if (card.isSelected) {
                        sendGameAction({
                            gameGuid: vue.gameGuid,
                            playerGuid: vue.player.playerGuid,
                            actionType: 'deckCard',
                            actionData: {
                                fromPlayerGuid: playerGuid,
                                fromKind: fromKind,
                                toPlayerGuid: playerGuid,
                                toKind: 'deck',
                                cardId: card.cardId,
                                cardState: {
                                    cardId: card.cardId,
                                    isFaceDown: card.isFaceDow,
                                }
                            }
                        });
                    }
                });
                    
                this.cardlist.sync.mustsync = false;
                this.cardlist.show = false;
            }
            
        },
       
    },
   
});

function playSoundFx(gameAction){
    if(vue.isStarted && gameAction.actionType) {
        var opponentPlaySound = gameAction.playerGuid === vue.opponent.playerGuid;
        var playerPlaySound = gameAction.playerGuid === vue.player.playerGuid;

        switch (gameAction.actionType) {
            case 'joinGame':
                soundIntro.play();
                break;
            case 'startGame':
                soundIntro.stop();
                // sound.play(gameAction.actionType);
                break;
            case 'drawCard':
                sound.play(gameAction.actionType);
                break;
            case 'discardCard':
                sound.play(gameAction.actionType);
                break;
            case 'playCard':
                sound.play(gameAction.actionType);
                break;
            case 'deckCard':
                sound.play(gameAction.actionType);
                break;
            case 'removeCard':
                sound.play(gameAction.actionType);
                break;
            case 'changeCardState':
                // sound.play(gameAction.actionType);
                var cardState = gameAction.actionData;
                if(cardState.isMarked !== undefined){
                    sound.play('rotateCard');
                }
                if(cardState.isFaceDown !== undefined){
                    sound.play('flipCard');
                }
                if (cardState.isPeeking !== undefined){
                   // sound.play('peekCard');
                } 
                if (cardState.isPointed !== undefined){
                   // sound.play('pointCard');
                } 
                if (cardState.top !== undefined && cardState.left !== undefined) {
                    // N.G. only play move animation with oponent cards
                    if(opponentPlaySound){
                        sound.play('moveCard');
                    }
                }
                break;
            case 'changeCurtainState':
                // N.G. only play blind waiting audio for current player
                var curtainState = gameAction.actionData;
                var playIntro = vue.player.playerGuid === curtainState.playerGuid;
                if (playIntro){
                    
                    // sound.play(gameAction.actionType);
                    if(curtainState.showCurtain){
                        soundIntro.play();
                    }
                    else {
                        soundIntro.stop();
                    }
                }
                
                break;
            case 'changePlayerStats':
                // sound.play(gameAction.actionType);
                break;
            case 'shuffleList':
                sound.play(gameAction.actionType);
                break;
            case 'updateList':
                // sound.play(gameAction.actionType);
                break;
            case 'flipCoin':
                sound.play(gameAction.actionType);
                break;
            case 'diceRoll': 
                sound.play(gameAction.actionType);
                break;
            case 'leaveGame':
                sound.play(gameAction.actionType);
                break;
    
        }
    } 

}

/* Process game actions*/
function processGameAction(gameAction) {
    console.log(JSON.stringify(gameAction, null, 2));
    playSoundFx(gameAction);
    switch (gameAction.actionType) {
        case 'joinGame':
            break;
        case 'startGame':
            processStartGame(gameAction.actionResult);
            break;
        case 'drawCard':
        case 'discardCard':
        case 'playCard':
        case 'deckCard':
        case 'removeCard':
            if(vue.isStarted) {
                processMoveCard(gameAction.actionData);
            }
            break;
        case 'changeCardState':
            if (vue.isStarted) {
                var animate = gameAction.playerGuid === vue.opponent.playerGuid;
                processChangeCardState(gameAction.actionData, animate);
            }
            break;

        case 'changeCurtainState':
            if (vue.isStarted) {
                processChangeCurtainState(gameAction.actionData);
            }
            break;
        case 'changePlayerStats':
            if(vue.isStarted) {
                processChangePlayerStats(gameAction.actionData);
            } 
            break;
        case 'shuffleList':
            if (vue.isStarted) {
                processShuffleList(gameAction.actionData, gameAction.actionResult);
            } 
            break;
        case 'updateList':
            if (vue.isStarted) {
                processUpdateList(gameAction.actionData, gameAction.actionResult);
            } 
            break;
        case 'flipCoin':
            if (vue.isStarted) {
                processFlipCoin(gameAction.actionData, gameAction.actionResult);
            } 
            break;
        case 'diceRoll': 
            if (vue.isStarted) {
                processDiceRoll(gameAction.actionData, gameAction.actionResult);
            } 
            break;
        case 'leaveGame':
            if (vue.isStarted) {
                processLeave(gameAction);
            }
            break;

    }
}

function processLeave(gameAction) {
    var mustShow = gameAction.playerGuid === vue.opponent.playerGuid;
    if (mustShow) {
        vue.curtainLeftText = 'Your opponent&nbsp;';
        vue.curtainRightText= 'left the arena...';
        vue.isStarted = false;
        vue.player.showCurtain = true;
    }

}

function setupJitsi(){
    if(!vue.services.jitsiApi) {
        vue.services.jitsiApi = new JitsiMeetExternalAPI(vue.jitsi.domain, vue.jitsi.options); 
        vue.services.jitsiApi.addListener('incomingMessage', incommingMessageHandler);
        vue.services.jitsiApi.addListener('outgoingMessage', outgoingMessageHandler);
        vue.services.jitsiApi.addListener('audioMuteStatusChanged', audioMuteStatusChangedHandler);
        vue.services.jitsiApi.addListener('videoMuteStatusChanged', videoMuteStatusChangedHandler);

        // vue.services.jitsiApi.executeCommand('avatarUrl', 'https://aminduna.arcmage.org/arcmage/Game/player.webp');
    }
}

/* Game bootstrap */
function init() {
    /* get the game id from the url */
    vue.gameGuid = $.urlParam('gameGuid');
    /* get the player id from the url */
    vue.player.playerGuid = $.urlParam('playerGuid');
    /* get the deck id from the url */
    vue.player.deckGuid = $.urlParam('deckGuid');
    vue.player.name =  $.urlParam('playerName');
    if (vue.player.name === undefined || vue.player.name === null) {
        vue.player.name = "Guest";
    }
    vue.player.name = decodeURIComponent(vue.player.name);

    vue.root = document.documentElement;

    /* configure jitsy */
    vue.jitsi.options.roomName = 'arcmage_' + vue.gameGuid;
    vue.jitsi.options.userInfo.displayName = vue.player.name;
    vue.jitsi.options.userInfo.email = 'player.webp'



    setupJitsi();
    

    window.onbeforeunload = function (e) {
        sendGameAction({
            gameGuid: vue.gameGuid,
            playerGuid: vue.player.playerGuid,
            actionType: 'leaveGame',
        });
        connection.close();
        /* $.connection.hub.stop(); */
    };

    window.addEventListener("keydown", e => {
        var prefix = "";

        if (e.ctrlKey){
            prefix += "Ctrl+";
        }

        if (e.shiftKey) {
            vue.keycode = "Shift+";
        }
       

        vue.keycode = prefix + e.key;

        vue.handleGlobalKeyPress();
    });
    window.addEventListener("keyup", e => {
        vue.keycode = null;
    });

    vue.restoreSettings();

    /* set up the web push api, and send the join game action on completion */
    /* when the join is successful, the processGameAction callback is called every time an action happens in the game */
    /* using the sendGameAction method, a action can be triggered on all clients */
    /* setup callback for game actions*/
    /* $.connection.games.client.processAction = processGameAction; */

    connection.on("ProcessAction", (gameAction) => { 
        processGameAction(gameAction); 
    });


    /* open communications hub and join game when it is up */
    /* $.connection.hub.start().done(joinGame); */
    connectHub();
}

/* start the signalr connection */
async function connectHub(){
    try {
        await connection.start();
        joinGame();
    } catch (err) {
    }
}

/* trigger game action on all clients */
async function sendGameAction(gameAction) {
    /* $.connection.games.server.pushAction(gameAction); */
    try {
        // Play sounds locally
        playSoundFx(gameAction);
        await connection.invoke("PushAction", gameAction);
    } catch (err) {
        console.error(err);
    }
}

/* join the game */
async function joinGame() {

    try {
        await connection.invoke("JoinGame", vue.gameGuid, vue.player.playerGuid, vue.player.name);    
    } catch (err) {
        console.error(err);
    }

    sendGameAction({
        gameGuid: vue.gameGuid,
        playerGuid: vue.player.playerGuid,
        actionType: 'loadDeck',
        actionData: { deckGuid: vue.player.deckGuid }
    });

}



/* Region: game actions */

/* Action game start */
function loadDeck(source, target) {
   
    $.each(source.deck.cards, function (index, card) {
        var c = createCard(card);
        vue.cards.push(c);
        target.deck.push(c);
    });
    $.each(source.play.cards, function (index, card) {
        var c = createCard(card);
        vue.cards.push(c);
        target.play.push(c);
        updateCardLocation(c, c.top, c.left, false);
    });

   
}

function createCard(card) {
    var imageUrlBase = portalUri;
    return {
        cardId: card.cardId,
        name: card.name,
        imageSrc: imageUrlBase + card.url,
        isMarked: card.isMarked,
        isDraggable: card.isDraggable,
        isFaceDown: card.isFaceDown,
        top: card.top,
        left: card.left,
        counterA: card.counterA,
        counterB: card.counterB,
        animateCardMove: false,
        ruleText: card.ruleText,
        flavorText: card.flavorText,
        subType: card.subType,
        isCity: card.isCity,
        isToken: card.isToken,
        isPeeking: false,
        isPointed: false
    };
}

function processStartGame(game) {
    var player = game.players.find(function (element) {
        return element.playerGuid === vue.player.playerGuid;
    });
    vue.player.name = player.name;
    loadDeck(player, vue.player);


    var opponent = game.players.find(function (element) {
        return element.playerGuid !== vue.player.playerGuid;
    });

    vue.opponent.playerGuid = opponent.playerGuid;
    vue.opponent.name = opponent.name;
    loadDeck(opponent, vue.opponent);

    setTimeout(function () {
        $("#player").css('background-image', 'url(' + player.avatar + ')');
        $("#opponent").css('background-image', 'url(' + opponent.avatar + ')');
        vue.isStarted = true;
        vue.curtainRightText = 'to open the blinds';
        vue.player.showCurtain = false;
        vue.opponent.showCurtain = false;
        sound.play('startGame');
    }, 1500);

   
}

/* Action dice roll */
function processDiceRoll(actionData, actionResult) {

    var element = document.getElementById('dice-outer-container');
    vue.diceRoll = true;
    var numberOfDice = 1;
    var valuesToThrow = [ actionResult ];
    var options = {
        element, // element to display the animated dice in.
        numberOfDice, // number of dice to use 
        values: valuesToThrow, // values to throw. When provided, overides library generated values. Optional.
    }
    rollADie(options);
    setTimeout(function () {
        vue.diceRoll = false;
    }, 3000);
}

/* Action flip a coin */
function processFlipCoin(updateListParam, headsOrTails) {
    vue.heads = headsOrTails === "Heads";

    vue.coinflip = true;
    setTimeout(function () {
        vue.coinflip = false;
    }, 3000);
}

/* Action move card form one list to another */
function processMoveCard(moveCardParam) {
    /* Nothing to do if the move destination is the same as the source*/
    if (moveCardParam.fromPlayerGuid === moveCardParam.toPlayerGuid &&
        moveCardParam.fromKind === moveCardParam.toKind) {
        processChangeCardState(moveCardParam.cardState, false);
        return;
    }

    var source = getList(moveCardParam.fromPlayerGuid, moveCardParam.fromKind);
    var card;
    if (moveCardParam.cardId !== undefined) {
        card = source.find(function (element) {
            return element.cardId === moveCardParam.cardId;
        });
        source.$remove(card);
    } else {
        card = source.pop();
    }
    if (card) {
        var target = getList(moveCardParam.toPlayerGuid, moveCardParam.toKind);
        if (moveCardParam.index !== undefined && moveCardParam.index !== -1) {
            target.splice(moveCardParam.index, 0, card);
        } else {
            target.push(card);
        }
        if (moveCardParam.toPlayerGuid === vue.player.playerGuid ){
            if (moveCardParam.toKind === 'hand') {
                vue.root.style.setProperty("--totalHandCards", Math.max(1, target.length));
            }
            if (moveCardParam.fromKind === 'hand') {
                vue.root.style.setProperty("--totalHandCards", Math.max(1, source.length));
            }
        }
       
        if (moveCardParam.cardState !== undefined) {
            moveCardParam.cardState.cardId = card.cardId;
            processChangeCardState(moveCardParam.cardState, false);
        }
    }
}

/* Action update a card's state (location, mark/unmark, faceUp/faceDown) */
function updateCardLocation(card, top, left, animate) {

    

    card.animateCardMove = animate;
    

    var isPlayerCard = vue.player.play.find(function (element) {
        return element.cardId === card.cardId;
    });
    if (isPlayerCard !== undefined) {
        card.top = snapToGrid(top, sizing.snap.gapY);
        card.left = snapToGrid(left, sizing.snap.gapX);
    }

    var isOpponentCard = vue.opponent.play.find(function (element) {
        return element.cardId === card.cardId;
    });
    /* mirror the location using the battlefield line as mirroring line if it's an opponent card */
    if (isOpponentCard !== undefined) {
        card.top = sizing.battlefield.height - snapToGrid(top, sizing.snap.gapY) - sizing.card.height;
        card.left = snapToGrid(left, sizing.snap.gapX);
    }
    if (isOpponentCard === undefined && isPlayerCard === undefined) {
        card.top = 0;
        card.left = 0;
    }
}

function snapToGrid(value, gap) {
    if (!vue.useGrid) return value;
    var modulus = value % gap;
    if (modulus < gap / 2) {
        return value - modulus;
    }
    return value - modulus + gap;
}

function processChangeCardState(state, animate) {
    var card = vue.cards.find(function (element) {
        return element.cardId === state.cardId;
    });
    if (card) {
        if (state.isMarked !== undefined) card.isMarked = state.isMarked;
        if (state.isDraggable !== undefined) card.isDraggable = state.isDraggable;
        if (state.isFaceDown !== undefined) card.isFaceDown = state.isFaceDown;
        if (state.counterA !== undefined) card.counterA = state.counterA;
        if (state.counterB !== undefined) card.counterB = state.counterB;
        if (state.isPeeking !== undefined) card.isPeeking = state.isPeeking;
        if (state.isPointed !== undefined) card.isPointed = state.isPointed;
        
        if (state.top !== undefined && state.left !== undefined) {
            updateCardLocation(card, state.top, state.left, animate);
        }
    }
}

/* Action update a player's stats (victory points, resources) */

/* updatePlayerStatsAction is a delayed triggered action, to bundle fast changes to 
  the resources/victory points of the player, before sending the action to all clients */
function updatePlayerStatsAction(playerGuid) {
    var player = getPlayer(playerGuid);
    if (player.statsTimer) {
        clearTimeout(player.statsTimer);
    }
    player.statsTimer = setTimeout(function () {
        sendGameAction({
            gameGuid: vue.gameGuid,
            playerGuid: vue.player.playerGuid,
            actionType: 'changePlayerStats',
            actionData: {
                playerGuid: player.playerGuid,
                victoryPoints: player.VictoryPoints,
                resources: {
                    black: {
                        used: player.resources.black.used,
                        available: player.resources.black.available,
                    },
                    blue: {
                        used: player.resources.blue.used,
                        available: player.resources.blue.available,
                    },
                    red: {
                        used: player.resources.red.used,
                        available: player.resources.red.available,
                    },
                    green: {
                        used: player.resources.green.used,
                        available: player.resources.green.available,
                    },
                    yellow: {
                        used: player.resources.yellow.used,
                        available: player.resources.yellow.available,
                    }
                }
            }
        });
    }, 1500);
}



function processChangeCurtainState(curtainState) {
    var player = getPlayer(curtainState.playerGuid);
    player.showCurtain = curtainState.showCurtain;
}


function processChangePlayerStats(playerState) {
    var player = getPlayer(playerState.playerGuid);
  
    player.resources.black.available = playerState.resources.black.available;
    player.resources.black.used = playerState.resources.black.used;
    player.resources.red.available = playerState.resources.red.available;
    player.resources.red.used = playerState.resources.red.used;
    player.resources.blue.available = playerState.resources.blue.available;
    player.resources.blue.used = playerState.resources.blue.used;
    player.resources.green.available = playerState.resources.green.available;
    player.resources.green.used = playerState.resources.green.used;
    player.resources.yellow.available = playerState.resources.yellow.available;
    player.resources.yellow.used = playerState.resources.yellow.used;
}

function processShuffleList(shuffleListParam, gamecards) {
    clearList(shuffleListParam.playerGuid, shuffleListParam.kind);
    var source = getList(shuffleListParam.playerGuid, shuffleListParam.kind);
    
    $.each(gamecards, function (index, gamecard) {
        var card = vue.cards.find(function (element) {
            return element.cardId === gamecard.cardId;
        });
        if (card !== undefined) {
            source.push(card);
        }
    });
}

function processUpdateList(updateListParam, gamecards) {

    if (updateListParam.kind === "Play") {

        // update the cards in play for the given player
        $.each(gamecards, function (index, gamecard) {
            processChangeCardState(gamecard, true);
        });
    } else {
        clearList(updateListParam.playerGuid, updateListParam.kind);
        var source = getList(updateListParam.playerGuid, updateListParam.kind);

        $.each(gamecards, function (index, gamecard) {
            var card = vue.cards.find(function (element) {
                return element.cardId === gamecard.cardId;
            });
            if (card !== undefined && card !== null) {
                card.isFaceDown = gamecard.isFaceDown;
                card.isMarked = gamecard.isMarked;
                card.isDraggable = gamecard.isDraggable;
                source.push(card);
            }
        });
    }
    // when dragging a card and updating a list at the same time, hand cards might have a top, left race condition.
    // this fixes that after the update
    $.each(vue.player.hand, function (index, gamecard) {
        processChangeCardState(
            {
                cardId: gamecard.cardId,
                top: 0,
                left:0,
            }, true);
    });

   
}

/* Region: helpers */

function audioMuteStatusChangedHandler(args){
    vue.jitsi.isAudioMuted = args.muted;
}

function videoMuteStatusChangedHandler(args){
    vue.jitsi.isVideoMuted = args.muted;
}


function incommingMessageHandler(args){
    vue.chatMessages.push(
        { 
            isOpponent: true,
            message: args.message
        }
    );
}



function outgoingMessageHandler(args){
    vue.chatMessages.push(
        { 
            isOpponent: false,
            message: args.message
        }
    );
}


function getPlayer(playerGuid) {
    if (vue.player.playerGuid === playerGuid) return vue.player;
    if (vue.opponent.playerGuid === playerGuid) return vue.opponent;
    return null;
}

function clearList(playerGuid, kind) {
    var player = getPlayer(playerGuid);
    if (player) {
        switch (kind) {
            case 'deck':
                player.deck = [];
                break;
            case 'graveyard':
                player.graveyard = [];
                break;
            case 'hand':
                player.hand = [];
                break;
            case 'play':
                player.play = [];
                break;
            case 'removed':
                player.removed = [];
                break;
        }
    }
}

function getList(playerGuid, kind) {
    var player = getPlayer(playerGuid);
    if (!player) return null;
    switch (kind) {
        case 'deck':
            return player.deck;
        case 'graveyard':
            return player.graveyard;
        case 'hand':
            return player.hand;
        case 'play':
            return player.play;
        case 'removed':
            return player.removed;
        default:
            return null;
    }
}
/* EndRegion: helpers*/

/* EndRegion: game actions */

/* Region: droppables */

function setDroppableState(isEnabled) {
    var state = isEnabled ? "enable" : "disable";
    $("#battleFieldDrop").droppable(state);
    $("#playerHand").sortable(state);
    $("#playerDeck").droppable(state);
    $("#playerGraveyard").droppable(state);
    $("#opponentHand").droppable(state);
    $("#opponentDeck").droppable(state);
    $("#opponentGraveyard").droppable(state);
}

function setupDropRegions() {
    
    $("#playerHand").sortable({
        //classes: {
        //    "ui-sortable": "droptarget"
        //},
        tolerance: 'perspectiveintersect',
       // tolerance: "pointer",
        start: function (event, ui) {
            var dragdata = $(ui.helper).data('dragdata');
            dragdata.oldIndex = vue.player.hand.findIndex(x => x.cardId === dragdata.item.cardId);
        },
        beforeStop: function (event, ui) {
            var dragdata = $(ui.helper).data('dragdata');
            if (dragdata.dropped) return true;
            if (!(dragdata.fromPlayerGuid === vue.player.playerGuid && dragdata.fromKind === 'hand')) {
                var index = -1;
                if (dragdata.fromKind === 'play') {
                    index = ui.item.index();
                    console.log('newindex: ' + index);
                }
                sendGameAction({
                    gameGuid: vue.gameGuid,
                    playerGuid: vue.player.playerGuid,
                    actionType: 'drawCard',
                    actionData: {
                        fromPlayerGuid: dragdata.fromPlayerGuid,
                        fromKind: dragdata.fromKind,
                        toPlayerGuid: vue.player.playerGuid,
                        toKind: 'hand',
                        cardId: dragdata.item.cardId,
                        index: index,
                        cardState: {
                            cardId: dragdata.item.cardId,
                            isFaceDown: false,
                            top: 0,
                            left: 0
                        }
                    }
                });
            } else {
                dragdata.top = 0;
                dragdata.left = 0;
                dragdata.item.top = 0;
                dragdata.item.left = 0;
                $(ui.helper).css({ top: 0, left: 0 });
                var newIndex = ui.item.index();
                vue.swapCards(vue.player.hand, dragdata.oldIndex, newIndex);
                console.log('oldindex: ' + dragdata.oldIndex + ' newindex: ' + newIndex + ' length: ' + vue.player.hand.length);
            }
            dragdata.dropped = true;
            return true;
        },
        // disable battelfield drop
        over: function (event, ui) {
            vue.highlightPlayerHand = true;
        },
        // enable battelfield drop
        out: function (event, ui) {
            vue.highlightPlayerHand = false;
        }
    });

    // Define opponenthand as a drop target, when a card it dropped, change it in the datastructures
    $("#opponentHand").droppable({
        classes: {
            "ui-droppable-hover": "droptarget"
        },
        tolerance: 'perspectiveintersect',
        drop: function (event, ui) {

            var dragdata = $(ui.helper).data('dragdata');
            if (dragdata.dropped) return true;
            if (!(dragdata.fromPlayerGuid === vue.opponent.playerGuid && dragdata.fromKind === 'hand')) {
                $(ui.helper).hide();
                dragdata.top = 0;
                dragdata.left = 0;
                dragdata.item.top = 0;
                dragdata.item.left = 0;
                $(ui.helper).css({ top: 0, left: 0 });
                sendGameAction({
                    gameGuid: vue.gameGuid,
                    playerGuid: vue.player.playerGuid,
                    actionType: 'drawCard',
                    actionData: {
                        fromPlayerGuid: dragdata.fromPlayerGuid,
                        fromKind: dragdata.fromKind,
                        toPlayerGuid: vue.opponent.playerGuid,
                        toKind: 'hand',
                        cardId: dragdata.item.cardId,
                        cardState: {
                            cardId: dragdata.item.cardId,
                            isFaceDown: false,
                            top: 0,
                            left: 0
                        }
                    }
                });

            }
            dragdata.dropped = true;
            return true;
        },
        // disable battelfield drop
        over: function (event, ui) {
  //          $("#battleField").droppable("disable");
        },
        // enable battelfield drop
        out: function (event, ui) {
  //          $("#battleField").droppable("enable");
        }
    });

    // Define player graveyard as a drop target, when a card it dropped, change it in the datastructures
    $("#playerGraveyard").droppable({
        classes: {
            "ui-droppable-hover": "droptarget"
        },
        tolerance: 'perspectiveintersect',
        drop: function (event, ui) {

            var dragdata = $(ui.helper).data('dragdata');
            if (dragdata.dropped) return true;
            if (!(dragdata.fromPlayerGuid === vue.player.playerGuid && dragdata.fromKind === 'graveyard')) {
                $(ui.helper).hide();
                dragdata.top = 0;
                dragdata.left = 0;
                dragdata.item.top = 0;
                dragdata.item.left = 0;
                $(ui.helper).css({ top: 0, left: 0 });
                sendGameAction({
                    gameGuid: vue.gameGuid,
                    playerGuid: vue.player.playerGuid,
                    actionType: 'discardCard',
                    actionData: {
                        fromPlayerGuid: dragdata.fromPlayerGuid,
                        fromKind: dragdata.fromKind,
                        toPlayerGuid: vue.player.playerGuid,
                        toKind: 'graveyard',
                        cardId: dragdata.item.cardId,
                        cardState: {
                            cardId: dragdata.item.cardId,
                            isFaceDown: false,
                            top: 0,
                            left: 0
                        }
                    }
                });

            }
            dragdata.dropped = true;
            return true;
        },
        // disable battelfield drop
        over: function (event, ui) {
  //          $("#battleField").droppable("disable");
        },
        // enable battelfield drop
        out: function (event, ui) {
  //          $("#battleField").droppable("enable");
        }
    });

    // Define player deck as a drop target, when a card it dropped, change it in the datastructures
    $("#playerDeck").droppable({
        classes: {
            "ui-droppable-hover": "droptarget"
        },
        tolerance: 'perspectiveintersect',
        drop: function (event, ui) {

            var dragdata = $(ui.helper).data('dragdata');
            if (dragdata.dropped) return true;
            if (!(dragdata.fromPlayerGuid === vue.player.playerGuid && dragdata.fromKind === 'deck')) {
                $(ui.helper).hide();
                dragdata.top = 0;
                dragdata.left = 0;
                dragdata.item.top = 0;
                dragdata.item.left = 0;
                $(ui.helper).css({ top: 0, left: 0 });
                sendGameAction({
                    gameGuid: vue.gameGuid,
                    playerGuid: vue.player.playerGuid,
                    actionType: 'deckCard',
                    actionData: {
                        fromPlayerGuid: dragdata.fromPlayerGuid,
                        fromKind: dragdata.fromKind,
                        toPlayerGuid: vue.player.playerGuid,
                        toKind: 'deck',
                        cardId: dragdata.item.cardId,
                        cardState: {
                            cardId: dragdata.item.cardId,
                            isFaceDown: true,
                            top: 0,
                            left: 0
                        }
                    }
                });

            }
            dragdata.dropped = true;
            return true;
        },
        // disable battelfield drop
        over: function (event, ui) {
  //          $("#battleField").droppable("disable");
        },
        // enable battelfield drop
        out: function (event, ui) {
   //         $("#battleField").droppable("enable");
        }
    });

    // Define opponent graveyard as a drop target, when a card it dropped, change it in the datastructures
    $("#opponentGraveyard").droppable({
        classes: {
            "ui-droppable-hover": "droptarget"
        },
        tolerance: 'perspectiveintersect',
        drop: function (event, ui) {

            var dragdata = $(ui.helper).data('dragdata');
            if (dragdata.dropped) return true;
            if (!(dragdata.fromPlayerGuid === vue.opponent.playerGuid && dragdata.fromKind === 'graveyard')) {
                $(ui.helper).hide();
                dragdata.top = 0;
                dragdata.left = 0;
                dragdata.item.top = 0;
                dragdata.item.left = 0;
                $(ui.helper).css({ top: 0, left: 0 });
                sendGameAction({
                    gameGuid: vue.gameGuid,
                    playerGuid: vue.player.playerGuid,
                    actionType: 'discardCard',
                    actionData: {
                        fromPlayerGuid: dragdata.fromPlayerGuid,
                        fromKind: dragdata.fromKind,
                        toPlayerGuid: vue.opponent.playerGuid,
                        toKind: 'graveyard',
                        cardId: dragdata.item.cardId,
                        cardState: {
                            cardId: dragdata.item.cardId,
                            isFaceDown: false,
                            top: 0,
                            left: 0
                        }
                    }
                });

            }
            dragdata.dropped = true;
            return true;
        },
        // disable battelfield drop
        over: function (event, ui) {
    //        $("#battleField").droppable("disable");
        },
        // enable battelfield drop
        out: function (event, ui) {
     //       $("#battleField").droppable("enable");
        }
    });

    // Define opponent deck as a drop target, when a card it dropped, change it in the datastructures
    $("#opponentDeck").droppable({
        classes: {
            "ui-droppable-hover": "droptarget"
        },
        tolerance: 'perspectiveintersect',
        drop: function (event, ui) {

            var dragdata = $(ui.helper).data('dragdata');
            if (dragdata.dropped) return true;
            if (!(dragdata.fromPlayerGuid === vue.opponent.playerGuid && dragdata.fromKind === 'deck')) {
                $(ui.helper).hide();
                dragdata.top = 0;
                dragdata.left = 0;
                dragdata.item.top = 0;
                dragdata.item.left = 0;
                $(ui.helper).css({ top: 0, left: 0 });
                sendGameAction({
                    gameGuid: vue.gameGuid,
                    playerGuid: vue.player.playerGuid,
                    actionType: 'deckCard',
                    actionData: {
                        fromPlayerGuid: dragdata.fromPlayerGuid,
                        fromKind: dragdata.fromKind,
                        toPlayerGuid: vue.opponent.playerGuid,
                        toKind: 'deck',
                        cardId: dragdata.item.cardId,
                        cardState: {
                            cardId: dragdata.item.cardId,
                            isFaceDown: true,
                            top: 0,
                            left: 0
                        }
                    }
                });

            }
            dragdata.dropped = true;
            return true;
        },
        // disable battelfield drop
        over: function (event, ui) {
      //      $("#battleField").droppable("disable");
        },
        // enable battelfield drop
        out: function (event, ui) {
    //        $("#battleField").droppable("enable");
        }
    });
    
    // Define the battelfield as a drop target, when a card it dropped, change it in the datastructures
    $("#battleFieldDrop").droppable({
        tolerance: 'perspectiveintersect',
        excludeRegions: [
            { name: '#playerDeck', top: sizing.playerDeck.top, left: sizing.playerDeck.left, width: sizing.playerDeck.width, height: sizing.playerDeck.height },
            { name: '#playerGraveyard', top: sizing.playerGraveyard.top, left: sizing.playerGraveyard.left, width: sizing.playerGraveyard.width, height: sizing.playerGraveyard.height },
            { name: '#opponentDeck', top: sizing.opponentDeck.top, left: sizing.opponentDeck.left, width: sizing.opponentDeck.width, height: sizing.opponentDeck.height },
            { name: '#opponentGraveyard', top: sizing.opponentGraveyard.top, left: sizing.opponentGraveyard.left, width: sizing.opponentGraveyard.width, height: sizing.opponentGraveyard.height },
            { name: '#opponentHand', top: sizing.opponentHand.top, left: sizing.opponentHand.left, width: sizing.opponentHand.width, height: sizing.opponentHand.height },
            { name: '#playerHand', top: sizing.playerHand.top, left: sizing.playerHand.left, width: sizing.playerHand.width, height: sizing.playerHand.height }],
        drop: function (event, ui) {

            var dragdata = $(ui.helper).data('dragdata');
            if (dragdata.dropped) return true;
            if (dragdata.fromKind !== "play") {
                sendGameAction({
                    gameGuid: vue.gameGuid,
                    playerGuid: vue.player.playerGuid,
                    actionType: 'playCard',
                    actionData: {
                        fromPlayerGuid: dragdata.fromPlayerGuid,
                        fromKind: dragdata.fromKind,
                        toPlayerGuid: vue.player.playerGuid,
                        toKind: 'play',
                        cardId: dragdata.item.cardId,
                        cardState: {
                            cardId: dragdata.item.cardId,
                            isFaceDown: dragdata.item.isFaceDown,
                            top: snapToGrid(dragdata.top,sizing.snap.gapY),
                            left: snapToGrid(dragdata.left, sizing.snap.gapX)
                        }
                    }
                });
            } else {
                if (dragdata.fromPlayerGuid === vue.player.playerGuid) {
                    Vue.nextTick(function() {
                        sendGameAction({
                            gameGuid: vue.gameGuid,
                            playerGuid: vue.player.playerGuid,
                            actionType: 'changeCardState',
                            actionData: {
                                cardId: dragdata.item.cardId,
                                top: snapToGrid(dragdata.top, sizing.snap.gapY),
                                left: snapToGrid(dragdata.left, sizing.snap.gapX)
                            }
                        });
                    });
                   
                }
                if (dragdata.fromPlayerGuid === vue.opponent.playerGuid) {
                    sendGameAction({
                        gameGuid: vue.gameGuid,
                        playerGuid: vue.player.playerGuid,
                        actionType: 'changeCardState',
                        actionData: {
                            cardId: dragdata.item.cardId,
                            top: sizing.battlefield.height - snapToGrid(dragdata.top, sizing.snap.gapY) - sizing.card.height,
                            left: snapToGrid(dragdata.left, sizing.snap.gapX)
                        }
                    });
                }
            }
            dragdata.dropped = true;
            return true;
        }
    });
}

/* EndRegion: droppables */
