(function () {
    const form = document.getElementById('preConsultForm');
    const prefix = document.getElementById('prefixSelect');
    const phone = document.querySelector('.phone-input');
    const PrefixWithPhone = document.getElementById('comparePrefixWithPhone');
    const msgPref = document.querySelector('[data-valmsg-for="PhonePrefixId"]');
    const msgPhone = document.querySelector('[data-valmsg-for="PhoneNumber"]');
    const msgEmail = document.querySelector('[data-valmsg-for="Email"]');
    const closeBtn = document.querySelector('#preConsultationModal .btn-close');
    const submitBtn = form.querySelector('button[type="submit"]');

    let prefixData;
    const loadPrefix = async () => {
        try {
            const res = await fetch('/Home/Prefix', { method: 'GET' });
            if (!res.ok) throw new Error('Server error: ' + res.status);
            prefixData = await res.json();

            if (prefix && prefix.options.length === 1) {
                fillPrefixOption(prefixData.prefixMetaById);
            }
        } catch (err) {
            console.error(err);
        }
    };

    $('#preConsultationModal').on('shown.bs.modal', function () {
        loadPrefix();
    });

    // ---- Select2
    $('.select2').select2({
        theme: 'bootstrap-5',
        width: 'resolve',
        dir: 'rtl',
        dropdownParent: $('#preConsultationModal'),
        minimumResultsForSearch: 6,
        placeholder: 'کشور'
    });

    // helper: meta for selected prefix
    function currentMeta() {
        const id = prefix.value;
        if (!id) return null;

        if (prefixData.prefixMetaById && prefixData.prefixMetaById[id]) {
            const m = prefixData.prefixMetaById[id];
            // support either camelCase or PascalCase just in case
            return { expectedLength: m.expectedLength ?? m.ExpectedLength, prefix: m.prefix ?? m.Prefix };
        }
        const opt = prefix.selectedOptions[0];
        if (!opt) return null;
        const exp = opt.getAttribute('data-expected');
        const pfx = opt.getAttribute('data-prefix');
        return exp ? { expectedLength: parseInt(exp, 10), prefix: pfx || '' } : null;
    }

    function fillPrefixOption(prefixMeta) {
        Object.values(prefixMeta).forEach(p => {
            const opt = document.createElement('option');
            opt.value = p.id;
            opt.textContent = `${p.countryName} (${p.prefix})`;
            prefix.appendChild(opt);
        });
    }

    // keep numeric only (no error messages while typing)
    phone.addEventListener('input', () => {
        const onlyDigits = phone.value.replace(/\D/g, '');
        if (phone.value !== onlyDigits) phone.value = onlyDigits;
    });

    // update maxlength/placeholder on prefix change (no errors yet)
    prefix.addEventListener('change', () => {
        const meta = currentMeta();
        if (meta && meta.expectedLength > 0) {
            phone.setAttribute('maxlength', meta.expectedLength);
            phone.setAttribute('inputmode', 'numeric');
            phone.placeholder = `مثلاً 9121234567 (بدون صفر، ${toFarsiDigits(String(meta.expectedLength))} رقم)`;
        } else {
            phone.removeAttribute('maxlength');
            phone.placeholder = 'مثلاً 9121234567 (بدون صفر)';
        }

        clearAllClientErrors();
    });

    function toFarsiDigits(str) {
        const map = ['۰', '۱', '۲', '۳', '۴', '۵', '۶', '۷', '۸', '۹'];
        return str.replace(/\d/g, d => map[d]);
    }

    function setError(el, text) {
        if (!el) return;
        el.textContent = text || '';
        if (text) phone.classList.add('is-invalid'); else phone.classList.remove('is-invalid');
    }
    function clearError(el) {
        if (!el) return;
        el.textContent = '';
        phone.classList.remove('is-invalid');
    }

    function clearAllClientErrors() {
        clearError(msgPref);
        clearError(msgPhone);
        clearError(PrefixWithPhone);
        clearError(msgEmail);
    }

    // validate ONLY when submitting
    function validateOnSubmit() {
        const meta = currentMeta();
        let ok = true;

        clearAllClientErrors();

        // prefix required
        if (!prefix.value) {
            setError(msgPref, 'انتخاب پیش‌شماره الزامی است.');
            ok = false;
        }

        // phone required + numeric
        if (!phone.value) {
            setError(msgPhone, 'شماره تماس الزامی است.');
            ok = false;
        } else if (!/^\d+$/.test(phone.value)) {
            setError(msgPhone, 'شماره باید فقط عدد باشد.');
            ok = false;
        } else if (meta && meta.expectedLength > 0 && phone.value.length !== meta.expectedLength) {
            const needFa = toFarsiDigits(String(meta.expectedLength));
            const hasFa = toFarsiDigits(String(phone.value.length));
            setError(PrefixWithPhone, `تعداد ارقام صحیح نیست. نیاز: ${needFa} رقم، فعلی: ${hasFa} رقم.`);
            ok = false;
        }

        return ok;
    }

    form.addEventListener('submit', async (e) => {
        e.preventDefault();           // stop jQuery/unobtrusive

        if (!validateOnSubmit()) return;

        const fd = new FormData(form);

        // Optional: disable button during request
        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.dataset.prevText = submitBtn.innerHTML;
            submitBtn.innerHTML = 'در حال ارسال...';
        }

        try {
            // Send as form-url-encoded (so MVC binds like a normal form post)
            const body = new URLSearchParams();
            for (const [k, v] of fd.entries()) body.append(k, v.toString());

            const res = await fetch(form.action, {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8'
                },
                body
            });

            const data = await res.json();

            if (!res.ok) {
                // server error (non-200)
                setError(PrefixWithPhone, 'خطای سرور. لطفاً دوباره تلاش کنید.');
                return;
            }

            if (data.ok) {
                // success UI
                // You can use your toast lib; for now simple alert:
                // toast.success(data.message)
                showToast(data.message, 'success');

                // reset + close modal
                form.reset();
                clearAllClientErrors();
                // reset select2
                $(prefix).val(null).trigger('change');
                // close
                const modal = bootstrap.Modal.getInstance(document.getElementById('preConsultationModal'));
                modal?.hide();
            } else if (data.errors) {
                // map backend ModelState errors to UI spans
                // keys are property names: "PhonePrefixId", "PhoneNumber", "Email"
                if (data.errors.PhonePrefixId?.length) setError(msgPref, data.errors.PhonePrefixId[0]);
                if (data.errors.PhoneNumber?.length) setError(msgPhone, data.errors.PhoneNumber[0]);
                if (data.errors.Email?.length) setError(msgEmail, data.errors.Email[0]);

                // generic place for summary if needed:
                if (!data.errors.PhonePrefixId && !data.errors.PhoneNumber && !data.errors.Email) {
                    setError(PrefixWithPhone, 'ورودی‌ها نامعتبر است.');
                }
            } else {
                setError(PrefixWithPhone, 'پاسخ نامعتبر از سرور.');
            }
        } catch (err) {
            console.error(err);
            showToast(data.message, 'danger', true);
            setError(PrefixWithPhone, 'خطا در ارتباط با سرور.');
        } finally {
            if (submitBtn) {
                submitBtn.disabled = false;
                submitBtn.innerHTML = submitBtn.dataset.prevText || 'ارسال';
            }
        }
    });

    if (closeBtn) {
        closeBtn.addEventListener('click', function () {
            if (form) {
                form.reset();
            }

            clearAllClientErrors();

            prefix.value = '';
            // notify Select2 to refresh its UI
            const event = new Event('change', { bubbles: true });
            prefix.dispatchEvent(event);
        });
    }
})();