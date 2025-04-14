
export interface GuestToken {
    guestToken: string;
    isAvailable: boolean;
}

export interface Payload {
    place_id: number;
    table_id:number;
    isAvailable: boolean;
    passphrase: string;
    role: string;
}