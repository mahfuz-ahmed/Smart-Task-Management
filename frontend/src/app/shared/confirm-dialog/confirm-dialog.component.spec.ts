import { TestBed } from '@angular/core/testing';
import { ConfirmDialogComponent } from './confirm-dialog.component';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { By } from '@angular/platform-browser';

describe('ConfirmDialogComponent', () => {
  let mockDialogRef: any;
  let mockData: any;

  beforeEach(async () => {
    mockDialogRef = {
      close: vi.fn()
    };
    mockData = {
      title: 'Test Title',
      message: 'Test Message',
      confirmText: 'Confirm text',
      cancelText: 'Cancel text'
    };

    await TestBed.configureTestingModule({
      imports: [ConfirmDialogComponent],
      providers: [
        { provide: MatDialogRef, useValue: mockDialogRef },
        { provide: MAT_DIALOG_DATA, useValue: mockData }
      ]
    }).compileComponents();
  });

  it('should create and display the input data', () => {
    const fixture = TestBed.createComponent(ConfirmDialogComponent);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h2')?.textContent).toContain('Test Title');
    expect(compiled.querySelector('p')?.textContent).toContain('Test Message');

    const buttons = fixture.debugElement.queryAll(By.css('button'));
    expect(buttons[0].nativeElement.textContent).toContain('Cancel text');
    expect(buttons[1].nativeElement.textContent).toContain('Confirm text');
  });

  it('should close with false when cancel is clicked', () => {
    const fixture = TestBed.createComponent(ConfirmDialogComponent);
    fixture.detectChanges();

    const buttons = fixture.debugElement.queryAll(By.css('button'));
    buttons[0].triggerEventHandler('click', null);

    expect(mockDialogRef.close).toHaveBeenCalledWith(false);
  });

  it('should close with true when confirm is clicked', () => {
    const fixture = TestBed.createComponent(ConfirmDialogComponent);
    fixture.detectChanges();

    const buttons = fixture.debugElement.queryAll(By.css('button'));
    buttons[1].triggerEventHandler('click', null);

    expect(mockDialogRef.close).toHaveBeenCalledWith(true);
  });
});
