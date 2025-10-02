// tagits fram i samband med AI
export function base64ToArrayBuffer(base64) {
    const binary = atob(base64);
    const len = binary.length;
    const bytes = new Uint8Array(len);
    for (let i = 0; i < len; i++) bytes[i] = binary.charCodeAt(i);
    return bytes.buffer;
}

export function arrayBufferToBase64(buffer) {
    const bytes = new Uint8Array(buffer);
    let binary = '';
    for (let i = 0; i < bytes.byteLength; i++) binary += String.fromCharCode(bytes[i]);
    return btoa(binary);
}

export async function importKeyFromBase64(base64Key) {
    const raw = base64ToArrayBuffer(base64Key);
    return await crypto.subtle.importKey(
        'raw',
        raw,
        { name: 'AES-GCM' },
        false,
        ['encrypt', 'decrypt']
    );
}

export function generateIv() {
    return crypto.getRandomValues(new Uint8Array(12));
}

export async function encryptAesGcm(cryptoKey, plaintext) {
    const iv = generateIv();
    const encoder = new TextEncoder();
    const pt = encoder.encode(plaintext);
    const cipherBuffer = await crypto.subtle.encrypt(
        { name: 'AES-GCM', iv: iv },
        cryptoKey,
        pt
    );
    return {
        ivBase64: arrayBufferToBase64(iv.buffer),
        cipherBase64: arrayBufferToBase64(cipherBuffer)
    };
}

export async function decryptAesGcm(cryptoKey, ivBase64, cipherBase64) {
    const ivBuf = base64ToArrayBuffer(ivBase64);
    const ctBuf = base64ToArrayBuffer(cipherBase64);
    const plainBuf = await crypto.subtle.decrypt(
        { name: 'AES-GCM', iv: new Uint8Array(ivBuf) },
        cryptoKey,
        ctBuf
    );
    return new TextDecoder().decode(plainBuf);
}