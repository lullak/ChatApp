import { importKeyFromBase64, encryptAesGcm, decryptAesGcm } from './aes.js';

// tagit en del ai hjälp här

(function () {
    let connection = null;
    let currentUser = null;
    let chats = {
        General: { name: "General Chat", messages: [], closed: false },
    };
    let activeChatKey = "General";

    // Sanitize function
    function sanitizeTextForSend(text) {
        return (typeof DOMPurify !== 'undefined')
            ? DOMPurify.sanitize(text, { ALLOWED_TAGS: [], ALLOWED_ATTR: [] })
            : text;
    }

    // ui chat
    function createNavLink(key, chat, isActive) {
        const navLink = document.createElement("a");
        navLink.className = `nav-link d-flex justify-content-between align-items-center ${isActive ? 'active' : ''}`;
        navLink.href = "#";

        const titleSpan = document.createElement("span");
        titleSpan.textContent = chat.name;
        titleSpan.style.flex = "1";
        titleSpan.onclick = (e) => {
            e.preventDefault();
            setActiveChat(key);
        };

        navLink.appendChild(titleSpan);

        if (key !== "General") {
            const closeBtn = document.createElement("button");
            closeBtn.type = "button";
            closeBtn.className = "btn btn-sm btn-danger ms-2";
            closeBtn.setAttribute("aria-label", `Close chat ${chat.name}`);
            closeBtn.textContent = "×";
            closeBtn.onclick = (e) => {
                e.preventDefault();
                e.stopPropagation();
                closeChat(key);
            };
            navLink.appendChild(closeBtn);
        }

        return navLink;
    }

    // Signalr setup
    function setupSignalR() {
        connection = new signalR.HubConnectionBuilder()
            .withUrl("/chathub")
            .withAutomaticReconnect()
            .build();

        connection.on("GeneralChatKey", (groupKeyBase64) => {
            if (!chats.General) chats.General = { name: "General Chat", messages: [], closed: false };

            importKeyFromBase64(groupKeyBase64)
                .then(key => { chats.General.cryptoKey = key; })
                .catch(() => {  });
        });

        connection.on("PrivateChatStarted", async (groupName, withUser, groupKeyBase64) => {
            await handlePrivateChatStarted(groupName, withUser, groupKeyBase64);
        });

        connection.on("UpdateUserList", handleUpdateUserList);

        connection.on("ReceiveMessage", async (sender, message) => {
            await handleReceiveMessageAll(sender, message);
        });

        connection.on("ReceivePrivateMessage", async (groupName, sender, message) => {
            await handleReceivePrivateMessage(groupName, sender, message);
        });

        return connection.start()
            .then(() => {
                const sendBtn = document.querySelector("#message-form button");
                if (sendBtn) sendBtn.disabled = false;
            });
    }

    // Ui and Dom
    function renderChatList() {
        const chatListEl = document.getElementById("chat-list");
        if (!chatListEl) return;
        chatListEl.innerHTML = "";
        Object.keys(chats).forEach(key => {
            const chat = chats[key];
            if (chat.closed) return;
            const isActive = key === activeChatKey;
            chatListEl.appendChild(createNavLink(key, chat, isActive));
        });
    }

    function renderActiveChat() {
        const chat = chats[activeChatKey];
        if (!chat) return;

        const activeChatNameEl = document.getElementById("active-chat-name");
        if (activeChatNameEl) activeChatNameEl.textContent = chat.name;

        const messagesListEl = document.getElementById("messages-list");
        if (!messagesListEl) return;
        messagesListEl.innerHTML = "";

        chat.messages.forEach(msg => {
            const div = document.createElement('div');
            const strong = document.createElement('strong');
            strong.textContent = msg.user === currentUser ? "You: " : `${msg.user}: `;
            div.appendChild(strong);
            div.appendChild(document.createTextNode(msg.content));
            messagesListEl.appendChild(div);
        });

        messagesListEl.scrollTop = messagesListEl.scrollHeight;
    }

    function setActiveChat(key) {
        if (activeChatKey === key) return;
        if (chats[key] && chats[key].closed) {
            chats[key].closed = false;
        }
        activeChatKey = key;
        renderChatList();
        renderActiveChat();
    }

    function closeChat(key) {
        if (key === "General") return;
        if (!chats[key]) return;

        chats[key].closed = true;
        if (activeChatKey === key) {
            activeChatKey = "General";
        }
        renderChatList();
        renderActiveChat();
    }
    // handlers
    function handleUpdateUserList(users) {
        const userListEl = document.getElementById("user-list");
        const onlineCountEl = document.getElementById("online-users-count");
        if (userListEl) userListEl.innerHTML = "";
        if (onlineCountEl) onlineCountEl.textContent = users.length;

        users.sort().filter(u => u !== currentUser).forEach(user => {
            const userItem = document.createElement("a");
            userItem.className = "list-group-item list-group-item-action";
            userItem.href = "#";
            userItem.textContent = user;
            userItem.onclick = (e) => {
                e.preventDefault();
                if (connection) connection.invoke("JoinPrivateChat", user);
            };
            if (userListEl) userListEl.appendChild(userItem);
        });
    }

    async function handleReceiveMessageAll(sender, message) {
        let content = message;
        if (typeof content === 'string' && content.startsWith('ENC:')) {
            const parts = content.split(':');
            if (parts.length === 3 && chats.General && chats.General.cryptoKey) {
                try {
                    const decrypted = await decryptAesGcm(chats.General.cryptoKey, parts[1], parts[2]);
                    content = (typeof DOMPurify !== 'undefined') ? DOMPurify.sanitize(decrypted, { ALLOWED_TAGS: [], ALLOWED_ATTR: [] }) : decrypted;
                } catch {
                    content = '[Encrypted message: failed to decrypt]';
                }
            } else {
                content = '[Encrypted message: no key]';
            }
        }

        if (!chats.General) chats.General = { name: "General Chat", messages: [], closed: false };
        chats.General.messages.push({ user: sender, content: content });
        if (activeChatKey === "General") {
            renderActiveChat();
        }
        renderChatList();
    }

    async function handlePrivateChatStarted(groupName, withUser, groupKeyBase64) {
        if (!chats[groupName]) {
            chats[groupName] = { name: `DM ${withUser}`, messages: [], closed: false, partner: withUser };
        } else {
            chats[groupName].closed = false;
            chats[groupName].name = `DM ${withUser}`;
            chats[groupName].partner = withUser;
        }

        try {
            const cryptoKey = await importKeyFromBase64(groupKeyBase64);
            chats[groupName].cryptoKey = cryptoKey;
        } catch {
        }

        renderChatList();
        setActiveChat(groupName);
    }

    async function handleReceivePrivateMessage(groupName, sender, message) {
        if (!chats[groupName]) {
            chats[groupName] = { name: `DM ${sender}`, messages: [], closed: false, partner: sender };
            renderChatList();
        }

        let content = message;
        if (typeof content === 'string' && content.startsWith('ENC:')) {
            const parts = content.split(':');
            if (parts.length === 3 && chats[groupName].cryptoKey) {
                try {
                    const decrypted = await decryptAesGcm(chats[groupName].cryptoKey, parts[1], parts[2]);
                    content = (typeof DOMPurify !== 'undefined') ? DOMPurify.sanitize(decrypted, { ALLOWED_TAGS: [], ALLOWED_ATTR: [] }) : decrypted;
                } catch {
                    content = '[Encrypted message: failed to decrypt]';
                }
            } else {
                content = '[Encrypted message: no key]';
            }
        }

        if (chats[groupName].closed && sender !== currentUser) {
            chats[groupName].closed = false;
            setActiveChat(groupName);
        }

        chats[groupName].messages.push({ user: sender, content });
        if (activeChatKey === groupName) renderActiveChat();
        renderChatList();
    }

    async function handleSendMessage(e) {
        e.preventDefault();
        const input = document.getElementById("message-input");
        if (!input) return;
        let text = input.value.trim();
        if (!text) return;

        const sanitized = sanitizeTextForSend(text);

        if (activeChatKey === "General") {
            const chat = chats.General;
            if (chat && chat.cryptoKey) {
                const { ivBase64, cipherBase64 } = await encryptAesGcm(chat.cryptoKey, sanitized);
                text = `ENC:${ivBase64}:${cipherBase64}`;
            } else {
                text = sanitized;
            }
            await connection.invoke("SendMessageAll", text);
        } else {
            const chat = chats[activeChatKey];
            if (chat && chat.cryptoKey) {
                const { ivBase64, cipherBase64 } = await encryptAesGcm(chat.cryptoKey, sanitized);
                text = `ENC:${ivBase64}:${cipherBase64}`;
            } else {
                text = sanitized;
            }
            await connection.invoke("SendPrivateMessage", activeChatKey, text);
        }

        input.value = "";
        input.focus();
    }

    function wireUpDomEvents() {
        const form = document.getElementById("message-form");
        if (form) form.addEventListener('submit', handleSendMessage);

        const sendBtn = document.querySelector("#message-form button");
        if (sendBtn) sendBtn.disabled = false;
    }

    // init 
    document.addEventListener('DOMContentLoaded', async () => {
        let username =
            window.currentUser ||
            (document.getElementById('chat-root')?.getAttribute('data-current-user')) ||
            document.getElementById('current-username')?.value ||
            document.querySelector('meta[name="current-user"]')?.content ||
            null;

        currentUser = username;

        renderChatList();
        renderActiveChat();

        wireUpDomEvents();

        await setupSignalR();
    });

})();