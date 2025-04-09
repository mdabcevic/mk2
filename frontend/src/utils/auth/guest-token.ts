
export interface GuestToken {
    token: string;
    isAvailable: boolean;
}

export interface Payload {
    place_id: number;
    isAvailable: boolean;
    passphrase: string;
    role: string;
}