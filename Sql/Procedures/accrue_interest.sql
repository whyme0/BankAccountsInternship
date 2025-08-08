-- PROCEDURE: public.accrue_interest(uuid)

-- DROP PROCEDURE IF EXISTS public.accrue_interest(uuid);

CREATE OR REPLACE PROCEDURE public.accrue_interest(
	IN account_id uuid)
LANGUAGE 'plpgsql'
AS $BODY$
DECLARE
    balance NUMERIC(18,2);
    interest_rate NUMERIC(5,2);
    closed_date TIMESTAMPTZ;
    days_elapsed INTEGER;
    accrued_interest NUMERIC(18,2);
    currency varchar(3);
BEGIN
    SELECT "Balance", "InterestRate", "ClosedDate", "Currency"
    INTO balance, interest_rate, closed_date, currency
    FROM public."Accounts"
    WHERE "Id" = account_id
    FOR UPDATE;

    days_elapsed := closed_date::date - CURRENT_DATE;

    IF days_elapsed <= 0 THEN
        RETURN;
    END IF;

    accrued_interest := balance * (interest_rate / 100) * (days_elapsed::numeric / 365);

    IF accrued_interest = 0 THEN
        RETURN;
    END IF;

    INSERT INTO public."Transactions" ("Id", "AccountId", "Amount", "Currency", "Type", "Date", "Description")
    VALUES (gen_random_uuid(), account_id, accrued_interest, currency, 1, CURRENT_TIMESTAMP, 'Начисление процентов');

    UPDATE public."Accounts"
    SET "Balance" = "Balance" + accrued_interest
    WHERE "Id" = account_id;
END;
$BODY$;
ALTER PROCEDURE public.accrue_interest(uuid)
    OWNER TO pguser;
