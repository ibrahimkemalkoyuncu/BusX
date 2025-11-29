// DİKKAT: Başındaki 'export' kelimesi olmazsa diğer dosyalar bunu göremez!
export interface Journey {
    id: number;
    fromCity: string;
    toCity: string;
    departure: string;
    arrivalEstimate: string;
    providerName: string;
    price: number;
}


export interface Seat {
    id: number;
    seatNumber: number;
    row: number;
    column: number;
    type: number; // 0: Koridor, 1: Cam, 2: Tekli
    isSold: boolean;
    genderLock: number | null; // 1: Erkek, 2: Kadın
    price: number;
}