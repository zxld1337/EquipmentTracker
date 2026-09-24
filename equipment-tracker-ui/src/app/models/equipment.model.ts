export interface Equipment {
  id: number;
  name: string;
  barcode: string;
  location: string;
  status: string;
  calibrationIntervalMonths: number;
  nextDueDate: string | null;
  isDue: boolean;
}

export interface EquipmentRequest {
  name: string;
  barcode: string;
  location: string;
  calibrationIntervalMonths: number;
}